using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Client");
            var ip=IPAddress.Parse("192.168.1.82");
            var port = 27001;
            var ep=new IPEndPoint(ip, port);
            using(var socket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    await socket.ConnectAsync(ep);
                    if (socket.Connected)
                    {
                        Console.WriteLine($"Client connected to server...");
                        while (true)
                        {
                            Console.Write("Enter Message:");
                            var message=Console.ReadLine();
                            if(string.IsNullOrEmpty(message))
                            {
                                continue;
                            }
                            var bytes=Encoding.UTF8.GetBytes(message);
                            await socket.SendAsync(bytes,SocketFlags.None);
                            if (message.Trim()=="exit")
                            {
                                Console.WriteLine("Disconnected from server...");
                                socket.Shutdown(SocketShutdown.Both);
                                break;
                            }

                        }
                    }
                }
                catch (SocketException ex)
                {

                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
