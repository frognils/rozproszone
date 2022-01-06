using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Server
{

    static void help()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nKomendy:\n");
        Console.WriteLine("/login <nazwa>\t\t\tzaloguj jako <nazwa>");
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void Send(String msg, byte[] b, UdpClient u)
    {
        Boolean incorrect = true;
        while (incorrect)
        {

            Console.Write(msg);
            string s = Console.ReadLine();
            if (s != "")
            {
                if (msg == "login: ") s = "/login " + s;
                else if (s == "/help") help();
                incorrect = false;
                b = Encoding.ASCII.GetBytes(s);
                u.Send(b, b.Length);
            }
        }
    }

    static void Msg(byte[] b, UdpClient u)
    {
        while (true)
        {
            Send("", b, u);
        }
    }
    static  async Task Receive(byte[] b, UdpClient u)
    {
        await Task.Run(() =>
        {
            string data;

            IPEndPoint other = new IPEndPoint(0, 0);
            while (true)
            {

                b = u.Receive(ref other);
                data = Encoding.ASCII.GetString(b, 0, b.Length);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.White;
            }

        });
    }



    public static void Main()
    {

        UdpClient sock = new UdpClient();
        sock.Connect("localhost", 2222);
        const int buf_size = 1024;
        byte[] bytes = new byte[buf_size];

        Send("login: ", bytes, sock);
        Receive(bytes, sock);
        while (true)
        {
            Send("", bytes, sock);
        } // end while



    } // end of Main()

}
