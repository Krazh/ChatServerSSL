using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EchoServerMultipleConnections
{

    // this class is used as a single connection to a tcp client
    class EchoConnection
    {
        private TcpClient _client;
        private StreamReader reader;
        private StreamWriter writer;
        private EchoServer echoServer;
        private string _userId;
        private bool running;
        private bool temp;
        private SslStream _sslStream;
        private SslProtocols _enabledSslProtocols;
        private NetworkStream _ns;

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public TcpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public EchoConnection(TcpClient client,EchoServer echoServer, string userId)
        {
            this.echoServer = echoServer;
            this.Client = client;
            UserId = userId;
            running = true;
            _ns = client.GetStream();
            string serverCertificateFile = "C:/certs/ServerSSL.cer";
            bool clientCertificateRequired = false;
            bool checkCertificateRevocation = true;
            _enabledSslProtocols = SslProtocols.Tls;
            X509Certificate serverCertificate = new X509Certificate(serverCertificateFile, "mysecret");
            bool leaveInnerStreamOpen = false;
            _sslStream = new SslStream(_ns, leaveInnerStreamOpen);
            try
            {
                _sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired, _enabledSslProtocols, checkCertificateRevocation);
                reader = new StreamReader(_sslStream);
                writer = new StreamWriter(_sslStream);
                temp = true;
                Console.WriteLine("Connected");
            }
            catch (Exception)
            {
                _sslStream.Dispose();
                _client.Close();
            }
        }

        public void Listen()
        {
            while (running)
            {
                try
                {
                    if (Client.Connected)

                    {
                        string receivedString = reader.ReadLine();
                        echoServer.IncomingMessage(receivedString, _userId);
                    }
                }
                catch (Exception)
                {
                    DisposeSocket();
                }

            }            
        }

        public void SendMessage(string message)
        {
            try
            {
                writer.WriteLine(message);
                writer.Flush();
            }
            catch (Exception)
            {
                DisposeSocket();
            }
        }

        public void DisposeSocket()
        {
            _sslStream.Close();
            _client.Close();
            running = false;
            echoServer.Connections.Remove(this);
        }

    }
}
