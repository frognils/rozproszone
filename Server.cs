using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
public class Server
{
    public static void Main()
    {
        Dictionary<IPEndPoint, string> users = new Dictionary<IPEndPoint, string>();

        UdpClient sock = new UdpClient(2222);
        const int buf_size = 1024;
        byte[] bytes = new byte[buf_size];
        string data = null;
        char com_char = '/';
        String nick;
        while (data != "\\quit")
        {
            IPEndPoint other = new IPEndPoint(0, 0);
            bytes = sock.Receive(ref other);
            data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

            if (data[0] == com_char)
            {
                String command = data.Split()[0];

                if (command == "/login")
                {
                    sock.Connect("localhost",other.Port);
                    nick = data.Split()[1];
                    users[other]=nick;
                    Console.WriteLine($"{other} logged in as {nick}");
                    bytes = Encoding.ASCII.GetBytes($"{nick} joined the server");
                    sock.Send(bytes, bytes.Length);
                }


            }
            else
            {

                Console.WriteLine($"Received: '{data}' from {users[other]}");
            }
        }
    }
}
