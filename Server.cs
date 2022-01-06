using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

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
            sock.Connect("localhost", address.Port);
            nick = data.Split()[1];
            users[address] = new User(nick);
            Console.WriteLine($"{address} logged in as {nick}");
            bytes = Encoding.ASCII.GetBytes($"{nick} joined the server\n");
            sock.Send(bytes, bytes.Length);
        }

        void createGroup()
        {
            sock.Connect("localhost", address.Port);
            string groupName = data.Split()[1];
            groups.Add(groupName);

            string serverMessage = $"{users[address].getName()} created group {groupName}";
            Console.WriteLine(serverMessage);
            bytes = Encoding.ASCII.GetBytes(serverMessage + "\n");
            sock.Send(bytes, bytes.Length);
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
            sock.Connect("localhost", address.Port);
            string listedGroups = "";

            for (int i = 0; i < _groups.Count; i++)
            {
                listedGroups = listedGroups + _groups[i] + '\n';
            }

            bytes = Encoding.ASCII.GetBytes(listedGroups);
            sock.Send(bytes, bytes.Length);
        }

        void joinGroup()
        {
            sock.Connect("localhost", address.Port);
            string groupName = data.Split()[1];
            users[address].addGroup(groupName);

            string serverMessage = $"{users[address].getName()} joined group {groupName}";
            Console.WriteLine(serverMessage);
            bytes = Encoding.ASCII.GetBytes(serverMessage + "\n");
            sock.Send(bytes, bytes.Length);
        }

        void addToGroup()
        {
            sock.Connect("localhost", address.Port);
            string userName = data.Split()[1];
            string groupName = data.Split()[2];

            User user = null;
            List<User> _users = new List<User>(users.Values);

            for (int i = 0; i < _users.Count;i++)
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
                bytes = Encoding.ASCII.GetBytes($"User {userName} not found\n");
            } 
            else if (!groups.Contains(groupName))
            {
                bytes = Encoding.ASCII.GetBytes($"Group {groupName} does not exist\n");
            }
            else
            {
                user.addGroup(groupName);
                serverMessage = $"{userName} was added to group {groupName} by {users[address].getName()}";
                Console.WriteLine(serverMessage);
                bytes = Encoding.ASCII.GetBytes(serverMessage + "\n");
            }

            sock.Send(bytes, bytes.Length);
        }
    }
}
