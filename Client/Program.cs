using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

//ONEMLI QEYD: Path-leri Client proyekti daxilindeki faylardan goture bilersiniz. 
class Client
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("====================================\n           FILE CLIENT            \n====================================");
        var ipAddress = IPAddress.Parse("192.168.1.82");
        var port = 27001;
        var ep = new IPEndPoint(ipAddress, port);

        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            try
            {
                
                await socket.ConnectAsync(ep);
                Console.WriteLine($"{socket.RemoteEndPoint}: Servere qosuldunuz..");

                while (true)
                {
                    
                    Console.WriteLine(">>> File Transferi <<<");
                    Console.WriteLine("exit - Cixis");
                    Console.WriteLine("===================================");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Faylin yolunu daxil edin: \n");
                    var filePath = Console.ReadLine()?.Trim();
                    Console.ResetColor();
                    if (filePath?.Trim()=="exit")
                    {
                        Console.WriteLine("Emeliyyat sonlandirildi");
                        break;
                    }
                    if (!File.Exists(filePath))//File -ni daxil edilen Path de olub-olmadigini yoxlayir
                    {
                        Console.ForegroundColor= ConsoleColor.DarkYellow;
                        Console.WriteLine("Bele bir fayl tapilmadi.! Yeniden daxil edin\n");
                        Console.ResetColor();

                        continue;
                    }

                    string fileName = Path.GetFileName(filePath);//daxil edilen faylin adi goturulur
                    byte[] nameBytes = Encoding.UTF8.GetBytes(fileName);//ve hemin adi byte-a ceviririk

                    long fileSize = new FileInfo(filePath).Length;//en boyuk olcu tipi ile Path-deki faylin olcusunu gotururuk
                    int min = 1024;
                    string result = ((fileSize / min) > 1024) ? $"{(fileSize / min / min)} MB" : $"{(fileSize / min)} KB";
                    Console.WriteLine($"Gonderilir: {fileName} : {result}...");

                    //Fayl adinin uzunlugunu gonderirik
                    await socket.SendAsync(BitConverter.GetBytes(nameBytes.Length), SocketFlags.None);

                    //Fayl adini gonderirik
                    await socket.SendAsync(nameBytes, SocketFlags.None);

                    //Fayl olcusunu gonderirik
                    await socket.SendAsync(BitConverter.GetBytes(fileSize), SocketFlags.None);

                    //Fayli oxuyub parca-parca servere otururuk
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))//File stream vasitesile faylimizi mueyyen prosesle byte -a cevirib servere gonderir.
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), SocketFlags.None);
                        }
                    }
                    Console.ForegroundColor= ConsoleColor.Green;
                    Console.WriteLine("Fayl ugurla gonderildi...\n");
                    Console.ResetColor();

                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Xeta: {ex.Message}");
                Console.ResetColor();
                Console.ResetColor();
            }
        }
    }
}