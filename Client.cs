using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Server
{

    public static void ClearCurrentConsoleLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop-1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop);
    }

    static void help()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nKomendy:");
        Console.WriteLine("/login <nazwa>\t\t\t\tzaloguj jako <nazwa>");
        Console.WriteLine("/creategroup <grupa>\t\t\tutwórz grupę <grupa>");
        Console.WriteLine("/cgroup <grupa>\t\t\t\tutwórz grupę <grupa>");
        Console.WriteLine("/groups\t\t\t\t\twyświetl istniejące grupy");
        Console.WriteLine("/mygroups\t\t\t\twyświetl grupy, do których należysz");
        Console.WriteLine("/join <grupa>\t\t\t\tdołącz do grupy <grupa>");
        Console.WriteLine("/add <nazwa> <grupa>\t\t\tdodaj użytkownika <nazwa> do grupy <grupa>");
        Console.WriteLine("/leave <grupa>\t\t\t\topuść grupę <grupa>");
        Console.WriteLine("/remove <nazwa> <grupa>\t\t\tusuń użytkownika <nazwa> z grupy <grupa>");
        Console.WriteLine("/groupmessage <grupa> <wiadomość>\twyślij <wiadomość> do grupy <grupa>");
        Console.WriteLine("/groupmmsg <grupa> <wiadomość>\t\twyślij <wiadomość> do grupy <grupa>");
        Console.WriteLine("/gm <grupa> <wiadomość>\t\t\twyślij <wiadomość> do grupy <grupa>");
        Console.WriteLine("/pm <użytkownik> <wiadomość>\t\t\twyślij <wiadomość> do użytkownika <użytkownik>");
        Console.WriteLine("/dm <użytkownik> <wiadomość>\t\t\twyślij <wiadomość> do użytkownika <użytkownik>");
        Console.ForegroundColor = ConsoleColor.White;
    }

    static void Send(String msg, byte[] b, UdpClient u)
    {
        char command_prefix = '/';
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
                ClearCurrentConsoleLine();
                //Console.SetCursorPosition(0, Console.CursorTop - 1);
                if (s[0] != command_prefix) Console.WriteLine($"[{DateTime.Now}]\t{s}");
                else if (s.Split()[0] == "/pm")
                    Console.WriteLine($"[{DateTime.Now}]\tto {s.Split()[1]}:{s.Replace(String.Join(" ", s.Split()[0], s.Split()[1]), "")}");
            }
        }
    }


    static async Task Receive(byte[] b, UdpClient u)
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
