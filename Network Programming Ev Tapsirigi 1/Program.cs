using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Server
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================\n           FILE SERVER          \n====================================");
        var ipAddress = IPAddress.Parse("192.168.1.82");
        var port = 27001;

        using (var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            var ep = new IPEndPoint(ipAddress, port);
            serverSocket.Bind(ep);
            serverSocket.Listen(5);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Server ise salindi...");
            Console.ResetColor();
            while (true)
            {
                var socket = await serverSocket.AcceptAsync();

                _ = Task.Run(async () =>
                {
                    Console.WriteLine($"\nMusteri ugurla qosuldu: {socket.RemoteEndPoint}\n");

                    try
                    {
                        while (true)
                        {

                            //Fayl adinin uzunlugunu oxuyuruq (4 bayt)
                            byte[] nameLenBuffer = new byte[4];
                            await socket.ReceiveAsync(nameLenBuffer, SocketFlags.None);
                            int nameLen = BitConverter.ToInt32(nameLenBuffer, 0);
                            foreach (var item in nameLenBuffer)
                            {
                                Console.Write($" {item} ");
                            }
                            Console.WriteLine();


                            //Fayl adini oxuyuruq
                            byte[] nameBuffer = new byte[nameLen];
                            await socket.ReceiveAsync(nameBuffer, SocketFlags.None);
                            string fileName = Encoding.UTF8.GetString(nameBuffer);
                            foreach (var item in nameBuffer)
                            {
                                Console.Write($" {item} ");
                            }
                            Console.WriteLine();
                            //Faylin olçusunu oxuyuruq (8 bayt - long)
                            byte[] sizeBuffer = new byte[8];
                            await socket.ReceiveAsync(sizeBuffer, SocketFlags.None);
                            long fileSize = BitConverter.ToInt64(sizeBuffer, 0);
                            foreach (var item in sizeBuffer)
                            {
                                Console.Write($" {item} ");
                            }
                            Console.WriteLine();
                            //Faylin saxlanilacagi qovluq
                            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReceivedFiles");
                            Directory.CreateDirectory(folderPath);
                            string savePath = Path.Combine(folderPath, fileName);

                            //Faylin mezmununu oxuyub diske yaziriq
                            using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[8192]; //8KB bufer pacetlenerek gonderilen melumatlari almaq ucun
                                long totalReceived = 0;

                                while (totalReceived < fileSize)
                                {
                                    int bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);
                                    if (bytesRead == 0) break;

                                    await fs.WriteAsync(buffer, 0, bytesRead);
                                    totalReceived += bytesRead;
                                }
                            }

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("[QEBUL EDILDI]   ");
                            Console.ResetColor();
                            Console.Write(fileName);
                            int min = 1024;
                            string result = ((fileSize / min) > 1024) ? $"{(fileSize / min / min)}MB" : $"{(fileSize / min)}KB";
                            Console.Write($"  {result}");
                            Console.Write($"  {DateTime.Now}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Xeta: {ex.Message}");
                    }
                    finally
                    {
                        socket.Dispose();
                    }


                });
            }
        }
    }
}