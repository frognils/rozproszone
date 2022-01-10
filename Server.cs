using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

public class User
{
    string name = "";
    IPEndPoint address;
    List<string> groups = new List<string>();

    public User(string name, IPEndPoint address)
    {
        this.name = name;
        this.address = address;
    }
    public User(string name, IPEndPoint address, List<string> groups)
    {
        this.name = name;
        this.address = address;
        this.groups = groups;
    }

    public string getName()
    {
        return this.name;
    }

    public IPEndPoint getAddress()
    {
        return this.address;
    }

    public List<string> getGroups()
    {
        return this.groups;
    }

    public Boolean isInGroup(string groupName)
    {
        return this.groups.Contains(groupName);
    }

    public void addGroup(string groupName)
    {
        this.groups.Add(groupName);
    }

    public void removeGroup(string groupName)
    {
        this.groups.Remove(groupName);
    }
}

public class Server
{
    public static void Main()
    {
        Dictionary<IPEndPoint, User> users = new Dictionary<IPEndPoint, User>();
        List<string> groups = new List<string>();


        const int buf_size = 1024;
        byte[] bytes = new byte[buf_size];
        string dataString = null;
        string[] data = null;
        char command_prefix = '/';
        String nick = "";
        IPEndPoint address;

        while (dataString != "\\quit")
        {
            UdpClient sock = new UdpClient();
            sock.ExclusiveAddressUse = false;
            sock.Client.Bind(new IPEndPoint(IPAddress.Loopback, 2222));
            address = new IPEndPoint(0, 0);
            bytes = sock.Receive(ref address);
            dataString = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            data = dataString.Split();

            if (dataString[0] == command_prefix) handleCommand();
            else handleRegularMessage();
            sock.Close();
        }

        async Task Send(IPEndPoint addr, string s, char color = 'y')
        {
            await Task.Run(() =>
            {
                UdpClient sender = new UdpClient(2222);
                sender.Connect("localhost", addr.Port);
                string dataToSend = $"[{DateTime.Now}] {s}";

                if (color != 'y')
                {
                    dataToSend = $":{color}:{dataToSend}";
                }

                bytes = Encoding.ASCII.GetBytes(dataToSend);
                sender.Send(bytes, bytes.Length);
                sender.Close();
                
            });

        }

        void handleRegularMessage()
        {
            Console.WriteLine($"{nick}: {dataString}");
            messageAll();
        }

        void handleCommand()
        {
            String command = data[0];

            switch (command)
            {
                case "/login": login(); break;
                case "/cgroup": // same as /creategroup
                case "/creategroup": createGroup(); break;
                case "/groups": listGroups(groups); break;
                case "/mygroups": listGroups(users[address].getGroups()); break;
                case "/join": joinOrLeaveGroup("join"); break;
                case "/add": addToOrRemoveFromGroup("add"); break;
                case "leave": joinOrLeaveGroup("leave"); break;
                case "/remove": addToOrRemoveFromGroup("remove"); break;
                case "/groupmessage": // same as /gm
                case "/groupmsg": // same as /gm
                case "/gm": messageGroup(); break;
                case "/dm": // same as /pm
                case "/pm": messagePriv(); break;
            }
        }

        void login()
        {
            nick = data[1];
            users[address] = new User(nick, address);
            Console.WriteLine($"{address} logged in as {nick}");

            Send(address, $"{nick} joined the server\n");
        }

        void createGroup()
        {
            string groupName = data[1];
            groups.Add(groupName);

            Console.WriteLine($"{nick} created group {groupName}");
            Send(address, $"You have created group {groupName}\n");
        }

        void listGroups(List<string> _groups)
        {
            string listedGroups = "";

            if (_groups.Count > 0)
            {
                for (int i = 0; i < _groups.Count; i++)
                {
                    listedGroups = listedGroups + _groups[i] + '\n';
                }
            }
            else
            {
                listedGroups = "There are no groups yet.\n";
            }

            Send(address, listedGroups);
        }

        void joinOrLeaveGroup(string operation)
        {
            if (operation != "join" && operation != "leave") return;

            string groupName = data[1];

            if (groups.Contains(groupName))
            {
                if (operation == "join")
                {
                    if (users[address].isInGroup(groupName))
                    {
                        Send(address, $"You are a member of this group already\n");
                    }
                    else
                    {
                        users[address].addGroup(groupName);
                        Console.WriteLine($"{nick} has joined group {groupName}");
                        Send(address, $"You have joined group {groupName}\n");
                    }
                }
                else
                {
                    if (users[address].isInGroup(groupName))
                    {
                        users[address].removeGroup(groupName);
                        Console.WriteLine($"{nick} has left group {groupName}");
                        Send(address, $"You have left group {groupName}\n");
                    }
                    else
                    {
                        Send(address, $"You are not a member of {groupName} group\n");
                    }
                }
            }
            else
            {
                Send(address, $"Group {groupName} does not exist\n");
            }
        }

        void addToOrRemoveFromGroup(string operation)
        {
            if (operation != "add" && operation != "remove") return;

            string userName = data[1];
            string groupName = data[2];
            string serverMessage;
            User user = findUserByName(userName);

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
                if (operation == "add")
                {
                    user.addGroup(groupName);
                    serverMessage = $"You have added {userName} to group {groupName}\n";
                    Console.WriteLine($"{userName} has been added to group {groupName} by {users[address].getName()}");
                }
                else
                {
                    user.removeGroup(groupName);
                    serverMessage = $"You have remove {userName} from group {groupName}\n";
                    Console.WriteLine($"{userName} has been removed from group {groupName} by {users[address].getName()}");
                }
            }

            Send(address, serverMessage);
        }

        void messageAll()
        {
            foreach (User user in users.Values)
            {
                User sendingUser = users[address];
                if (user != sendingUser)
                {
                    Task sending = Send(user.getAddress(), String.Join(":", sendingUser.getName(), data));
                    sending.Wait();
                }
            }
        }

        void messagePriv()
        {
            string userName = data[1];
            string serverMessage;
            User recivingUser = findUserByName(userName);
            User sendingUser = users[address];

            List<string> messageList = new List<string>(data);
            messageList.RemoveRange(0, 2);
            if (messageList == null || data.Length < 3)
            {
                serverMessage = $"Message can't be empty\n";
                Send(address, serverMessage);
            }
            else
            {
                if (recivingUser == null)
                {
                    serverMessage = $"User {userName} not found\n";
                    Send(address, serverMessage);
                }
                else if (recivingUser != sendingUser)
                {
                    string message = listToString(messageList, " ");
                    serverMessage = $"{sendingUser.getName()}: {message}";
                    Task sending = Send(recivingUser.getAddress(), serverMessage, 'c');
                    sending.Wait();
                }
                else
                {
                    serverMessage = $"You can't send message yourself\n";
                    Send(address, serverMessage);
                }
            }

        }

        void messageGroup()
        {
            string groupName = data[1];
            List<string> messageList = new List<string>(data);
            messageList.RemoveRange(0, 2); // index 0: command, index 1: groupName, from index 2 it is message
            string message = listToString(messageList, " ");

            if (users[address].isInGroup(groupName))
            {
                List<User> usersInGroup = findUsersByGroup(groupName);
                List<IPEndPoint> addressesList = new List<IPEndPoint>();
                string serverMessage = $"{nick}({groupName}): {message}";

                Console.WriteLine(serverMessage);

                usersInGroup.ForEach(async user =>
                {
                    Task sending = Send(user.getAddress(), serverMessage + "\n", 'g');
                    sending.Wait();
                });
            }
            else
            {
                Send(address, $"You are not a member of {groupName} group\n");
            }
        }

        User findUserByName(string userName)
        {
            User user = null;
            List<User> usersList = new List<User>(users.Values);

            for (int i = 0; i < usersList.Count; i++)
            {
                if (usersList[i].getName() == userName)
                {
                    return usersList[i];
                }
            }

            return null;
        }

        List<User> findUsersByGroup(string groupName)
        {
            List<User> usersList = new List<User>(users.Values);
            List<User> usersInGroupList = new List<User>();

            for (int i = 0; i < usersList.Count; i++)
            {
                User currentUser = usersList[i];

                if (currentUser.getGroups().Contains(groupName))
                {
                    usersInGroupList.Add(currentUser);
                }
            }

            return usersInGroupList;
        }

        string listToString(List<string> list, string delimiter = "")
        {
            string s = "";

            for (int i = 0; i < list.Count; i++)
            {
                s += list[i] + delimiter;
            }

            return s.Substring(0, s.Length - delimiter.Length);
        }
    }
}
