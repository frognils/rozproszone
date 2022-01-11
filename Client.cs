using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class Server
{

    public static void ClearCurrentConsoleLine()
    {
        Console.SetCursorPosition(0, Console.CursorTop -1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop - 1);
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
                if (msg.Contains("login: ")) s = "/login " + s;
                else if (s == "/help") help();
                incorrect = false;
                b = Encoding.ASCII.GetBytes(s);
                u.Send(b, b.Length);
                ClearCurrentConsoleLine();
                //Console.SetCursorPosition(0, Console.CursorTop - 1);
                if (s[0] != command_prefix) Console.WriteLine($"[{DateTime.Now}] {s}");
                else if (s.Split()[0] == "/pm" || s.Split()[0] == "/dm") 
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[{DateTime.Now}] to {s.Split()[1]}:{s.Replace(String.Join(" ", s.Split()[0], s.Split()[1]), "")}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
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
                char firstChar = data[0];
                string message = data;
                ConsoleColor color = ConsoleColor.Yellow;

                if (message == "This name is already taken") {
                    printLogin(b, u);
                    return;
                }

                // different coloring, used in direct and group messages
                if (firstChar == ':') // then it is something like ":c:Text" instead of just "Text"
                {
                    message = message.Substring(3); // 0 ":", 1: color char, 2 ":"

                    switch (data[1])
                    {
                        case 'c': color = ConsoleColor.Cyan; break;
                        case 'g': color = ConsoleColor.Green; break;
                        case 'w': color = ConsoleColor.White; break;
                    }
                }

                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }

        });
    }

    static void printLogin(byte[] bytes, UdpClient sock)
    {
        Send("login: ", bytes, sock);
        Receive(bytes, sock);
    }

    public static void Main()
    {

        UdpClient sock = new UdpClient();
        sock.Connect("localhost", 2222);
        const int buf_size = 1024;
        byte[] bytes = new byte[buf_size];

        printLogin(bytes, sock);
       
        while (true)
        {
            Send("", bytes, sock);
        } // end while



    } // end of Main()

}
