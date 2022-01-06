using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

public class User
{
    string name = "";
    List<string> groups = new List<string>();

    public User(string name)
    {
        this.name = name;
    }
    public User(string name, List<string> groups)
    {
        this.name = name;
        this.groups = groups;
    }

    public string getName()
    {
        return this.name;
    }

    public List<string> getGroups()
    {
        return this.groups;
    }

    public void addGroup(string groupName)
    {
        this.groups.Add(groupName);
    }
}

public class Server

{
    public static void Main()
    {
        Dictionary<IPEndPoint, User> users = new Dictionary<IPEndPoint, User>();
        List<string> groups = new List<string>();

        UdpClient sock = new UdpClient(2222);
        const int buf_size = 1024;
        byte[] bytes = new byte[buf_size];
        string data = null;
        char command_prefix = '/';
        String nick;
        IPEndPoint address;
        while (data != "\\quit")
        {
            address = new IPEndPoint(0, 0);
            bytes = sock.Receive(ref address);
            data = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

            if (data[0] == command_prefix) handleCommand();
            else handleRegularMessage();
        }

        void Send(IPEndPoint addr, String s)
        {
            UdpClient sender = new UdpClient();
            sender.Connect("localhost", addr.Port);
            bytes = Encoding.ASCII.GetBytes(s);
            sender.Send(bytes, bytes.Length);
            sender.Close();
        }

        void handleRegularMessage()
        {
            Console.WriteLine($"Received: '{data}' from {users[address]}");
        }

        void handleCommand()
        {
            String command = data.Split()[0];

            switch (command)
            {
                case "/login": login(); break;
                case "/creategroup": createGroup(); break;
                case "/groups": listAllGroups(); break;
                case "/mygroups": listMyGroups(); break;
                case "/join": joinGroup(); break;
                case "/add": addToGroup(); break;
            }
        }

        void login()
        {
            
            nick = data.Split()[1];
            users[address] = new User(nick);
            Console.WriteLine($"{address} logged in as {nick}");

            Send(address, $"{nick} joined the server\n");
        }

        void createGroup()
        {
            
            string groupName = data.Split()[1];
            groups.Add(groupName);

            string serverMessage = $"{users[address].getName()} created group {groupName}";
            Console.WriteLine(serverMessage);

            Send(address, serverMessage);
        }

        void listAllGroups()
        {
            listGroups(groups);
        }

        void listMyGroups()
        {
            listGroups(users[address].getGroups());
        }

        void listGroups(List<string> _groups)
        {
            
            string listedGroups = "";

            for (int i = 0; i < _groups.Count; i++)
            {
                listedGroups = listedGroups + _groups[i] + '\n';
            }

            Send(address, listedGroups);
        }

        void joinGroup()
        {
            
            string groupName = data.Split()[1];
            users[address].addGroup(groupName);

            string serverMessage = $"{users[address].getName()} joined group {groupName}";
            Console.WriteLine(serverMessage);

            Send(address, serverMessage + "\n");
        }

        void addToGroup()
        {
            
            string userName = data.Split()[1];
            string groupName = data.Split()[2];

            User user = null;
            List<User> _users = new List<User>(users.Values);

            for (int i = 0; i < _users.Count; i++)
            {
                if (_users[i].getName() == userName)
                {
                    user = _users[i];
                    break;
                }
            }

            string serverMessage;

            if (user == null)
            {
                serverMessage = $"User {userName} not found\n";
            }
            else if (!groups.Contains(groupName))
            {
                serverMessage = $"Group {groupName} does not exist\n";
            }
            else
            {
                user.addGroup(groupName);
                serverMessage = $"{userName} was added to group {groupName} by {users[address].getName()}";
                Console.WriteLine(serverMessage);
            }

            Send(address, serverMessage);

        }
    }
}
