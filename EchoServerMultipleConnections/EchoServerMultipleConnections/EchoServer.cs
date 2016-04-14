using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoServerMultipleConnections
{

    class EchoServer
    {
        // set port number and ip
        int portNumber = 6789;
        IPAddress ip = IPAddress.Any;
        private TcpClient _client;
        private string helpCommands;

        public List<EchoConnection> Connections = new List<EchoConnection>();

        public EchoServer()
        {
            TcpListener listener = new TcpListener(ip, portNumber);
            listener.Start();
            Trace.Listeners.Add(new CustomTextFileTrace());
            Trace.WriteLine("Server started");
            helpCommands =
                "Alias: Followed by your desired Username. Eg: alias TheMan\nQuit: Leave the server - this is not recommended ;)";
            while (true)
            {
                Console.WriteLine("Waiting for a connection - number of connections: {0}", Connections.Count);
                _client = listener.AcceptTcpClient();
                Random rnd = new Random();
                int uId = (Connections.Count + 1) * rnd.Next(1, 50000);
                EchoConnection conn = new EchoConnection(_client, this, uId.ToString());
                Connections.Add(conn);
                conn.SendMessage("Welcome stranger. Currently there are " + Connections.Count + " people online. \n Type 'help' for a list of options");
                Task.Factory.StartNew(() => conn.Listen());
                SendMessageToAllUsers("userconnected;" + uId);
            }
        }

        public void IncomingMessage(string receivedString, string userId)
        {
            Console.WriteLine("Incoming message: {0}", receivedString);
            var found = Connections.FirstOrDefault(x => x.UserId == userId);
            var i = Connections.IndexOf(found);
            string[] array = receivedString.Split(' ');
            receivedString = Connections[i].UserId + ": " + receivedString;
            switch (array[0].ToLower())
            {
                case "help":
                    Connections[i].SendMessage("\n" + helpCommands);
                    break;
                case "alias":
                    SetUserId(userId, array[1]);
                    break;
                case "quit":
                    Connections[i].SendMessage("quit");
                    Connections[i].DisposeSocket();
                    Console.WriteLine("{0} left the server", userId);
                    RemoveConnection(userId);
                    break;
                case "getallusers":
                    string allUsers = "";
                    foreach (var echoConnection in Connections)
                    {
                        allUsers += echoConnection.UserId + ";";
                    }
                    Connections[i].SendMessage("allusers;" + allUsers);
                    break;
                default:
                    SendMessageToAllUsers(receivedString);
                    break;
            }
        }

        private void SendMessageToAllUsers(string message)
        {
            foreach (EchoConnection echoConnection in Connections)
            {
                echoConnection.SendMessage(message);
            }
        }

        public void RemoveConnection(string uId)
        {
            var found = Connections.FirstOrDefault(x => x.UserId == uId);
            Connections.Remove(found);
            string activeConnections = "Number of connections: " + Connections.Count;
            Console.WriteLine(activeConnections);
            string userQuit = "userdisconnected;" + uId + ";";
            SendMessageToAllUsers(userQuit);
        }

        public bool SetUserId(string userId, string newName)
        {
            var found = Connections.FindAll(x => x.UserId == newName);
            var thisConnection = Connections.FirstOrDefault(x => x.UserId == userId);
            var j = Connections.IndexOf(thisConnection);
            
            while (true)
            {
                if (found.Count == 0)
                {
                    Connections[j].UserId = newName;
                    Connections[j].SendMessage("Username changed");
                    string msg = "userchangedname;" + userId + ";" + newName;
                    SendMessageToAllUsers(msg);
                    return true;
                }
                else
                {
                    Connections[j].SendMessage("Username already selected. Please try another.");
                    return false;
                }
                
            }
        }
    }
}