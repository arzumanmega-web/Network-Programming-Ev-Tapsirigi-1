using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Network_Programming_Ev_Tapsirigi_1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Server");
            var ip=IPAddress.Parse("192.168.1.82");
            var port = 27001;
            var ep = new IPEndPoint(ip,port);
            using(var socket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(ep);
                socket.Listen(10);
                Console.WriteLine($"Listen: {socket.LocalEndPoint}");
                while (true)
                {

                    var client = await socket.AcceptAsync();

                    _ = Task.Run(async () =>
                    {
                        Console.WriteLine($"Coonected client: {client.RemoteEndPoint}");
                        var bytes = new byte[1024];
                        try
                        {
                            while (true)
                            {
                                int length =await client.ReceiveAsync(bytes, SocketFlags.None);
                                if (length == 0)
                                {
                                    Console.WriteLine($"Client discoonected: {client.RemoteEndPoint}");
                                    break;
                                }
                                var message=Encoding.UTF8.GetString(bytes,0,length);
                                Console.WriteLine($"Client message: {message} , from {client.RemoteEndPoint}");
                                if (message.Trim()=="exit")
                                {
                                    Console.WriteLine($"Client discoonected: {client.RemoteEndPoint}");
                                    break;
                                }

                            }
                        }
                        catch (SocketException)
                        {
                            Console.WriteLine("Client disconnected server...");
                        }
                        catch (ObjectDisposedException)
                        {
                            Console.WriteLine("Client closed server...");
                        }
                        finally 
                        { 
                            client.Close(); 
                        }
                    });
                }
            }
        }
    }
}
