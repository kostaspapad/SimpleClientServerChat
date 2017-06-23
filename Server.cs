using System.Windows;
using System.Text;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml.Serialization;
namespace serverFromZero {
    class Server {
        
        private TcpListener _tcpListener;
        public Action<byte[], Client> OnDataReceive;

        private readonly object _token = new object();
        public bool Running { get; set; }
        
        //Create instance of client manager.
        public ClientManager CM = new ClientManager();

        //Server consructor.
        public Server(int port) {
        try {
                _tcpListener = new TcpListener(IPAddress.Any, port);
            }
            catch(Exception e) {
                CommandLine.Write(e.Message);
            }
            startServer();
        }

        //Start server listener thread.
        public void startServer() {
            Thread serverThread = new Thread(new ThreadStart(startListening));
            serverThread.Start();
        }

        public void startListening() {
            try {             
                _tcpListener.Start();
                Running = true;
                CommandLine.Write("Started listening at " + _tcpListener.Server.LocalEndPoint);
            }
            catch(Exception e) {
                CommandLine.Write(e.Message);
            }

            while (Running)
            {
                if (_tcpListener.Pending())
                {
                    //Non-blocking method determines if there are any pending connection requests.
                    //Inserting client to list.
                    CM.AddUser(new Client(_tcpListener.AcceptTcpClient()));
                }
            }
        }

        public void StopListening() {
            try {
                Running = false;
                _tcpListener.Stop();
                CommandLine.Write("Listener stopped.");
            }
            catch(Exception e) {
                CommandLine.Write(e.Message);
            }
        }

        public void Send(Client client, string data) {
            if(client == null || !client.Connected) return;

            var msg = new Message(data);

            try {
                client.stream.Write(msg.Data, 0, msg.Data.Length);
            }
            catch(Exception e) {
                CommandLine.Write(e.Message);
            }
        }

        public void SendAll(string data) {
            foreach(var entry in CM.users) { Send(entry, data); }
        }
    }
}
