// Dominion - Copyright (C) Timothy Ings
// NetServer.cs
// This file contains classes that define net server

using ArwicEngine.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArwicEngine.Net
{
    public class NetServerStats
    {
        /// <summary>
        /// Gets the size of the net client's recieve buffer
        /// </summary>
        public int RecieveBufferSize { get; set; }

        /// <summary>
        /// Gets the size of the net client's send buffer
        /// </summary>
        public int SendBufferSize { get; set; }

        /// <summary>
        /// Gets the number of packets recieved by the net client
        /// </summary>
        public int PacketsRecieved { get; set; }

        /// <summary>
        /// Gets the number of packets sent by the net client
        /// </summary>
        public int PacketsSent { get; set; }

        /// <summary>
        /// Gets the number of bytes recieved by the net client
        /// </summary>
        public int BytesRecieved { get; set; }

        /// <summary>
        /// Gets the number of bytes send by the net client
        /// </summary>
        public int BytesSent { get; set; }

        /// <summary>
        /// Gets the port the net server is listening on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets the number of clients currently connected to the net server
        /// </summary>
        public int ConnectionCount { get; set; }
    }

    public class ConnectionEventArgs : EventArgs
    {
        public Connection Connection { get; }

        public ConnectionEventArgs(Connection conn)
        {
            Connection = conn;
        }
    }

    public class NetServer
    {
        /// <summary>
        /// Gets or sets an object that tracks this server's statistics
        /// </summary>
        public NetServerStats Statistics { get; private set; } 

        /// <summary>
        /// Gets a value indicating whether the serve is currently running
        /// </summary>
        public bool Running { get; private set; }
        
        /// <summary>
        /// Gets an array of the clients currently conected to the server
        /// </summary>
        public Connection[] Connections
        {
            get
            {
                return connections.ToArray();
            }
        }
        private List<Connection> connections;

        private TcpListener listener;
        private int port;
        private CancellationTokenSource listenNewClientsToken;

        #region Events
        /// <summary>
        /// Occurs when the server recieves a packet
        /// </summary>
        public event EventHandler<PacketRecievedEventArgs> PacketRecieved;
        /// <summary>
        /// Occurs when the server accepts a new client
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionAccepted;
        /// <summary>
        /// Occurs when the server loses connection to a client
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionLost;
        #endregion

        #region Event Handlers
        protected virtual void OnPacketRecieved(PacketRecievedEventArgs args)
        {
            if (PacketRecieved != null)
                PacketRecieved(this, args);
        }
        protected virtual void OnConnectionAccepted(ConnectionEventArgs args)
        {
            if (ConnectionAccepted != null)
                ConnectionAccepted(this, args);
        }
        protected virtual void OnConnectionLost(ConnectionEventArgs args)
        {
            if (ConnectionLost != null)
                ConnectionLost(this, args);
        }
        #endregion

        /// <summary>
        /// Creates a new net server
        /// </summary>
        /// <param name="e"></param>
        public NetServer()
        {
            Running = false;
            connections = new List<Connection>();
            Statistics = new NetServerStats();
        }

        /// <summary>
        /// Starts the server, allowing it to accept connections and transfer data to and from clients
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            if (Running)
                throw new MethodAccessException("Cannot call NetServer.Start() while it is running");
            Running = true;

            this.port = port;

            Statistics.Port = port;

            ConsoleManager.Instance.WriteLine($"Starting server on {port}", MsgType.ServerInfo);
            listener = new TcpListener(IPAddress.Any, port);
            Statistics.RecieveBufferSize = listener.Server.ReceiveBufferSize;
            Statistics.SendBufferSize = listener.Server.SendBufferSize;
            listener.Start(100);
            ConsoleManager.Instance.WriteLine($"Listening on {port}", MsgType.ServerInfo);

            AcceptClients();
        }

        /// <summary>
        /// Stops the server, closing all client connections and preventing new ones after the given time
        /// </summary>
        public void Stop(int time)
        {
            Task.Run(() =>
            {
                Thread.Sleep(time);
                Stop();
            });
        }

        /// <summary>
        /// Stops the server, closing all client connections and preventing new ones
        /// </summary>
        public void Stop()
        {
            Running = false;
            listenNewClientsToken.Cancel();
            foreach (Connection conn in Connections)
            {
                try
                {
                    if (conn.Client.GetState() == System.Net.NetworkInformation.TcpState.Established)
                    {
                        conn.Client.Close();
                    }
                }
                catch (Exception) { }
            }
            Statistics.ConnectionCount = connections.Count;
            listener.Stop();
        }

        private async void AcceptClients()
        {
            listenNewClientsToken = new CancellationTokenSource();
            while (Running)
            {
                try
                {
                    // await a new client, with cancelation
                    TcpClient client = await listener.AcceptTcpClientAsync().WithCancellation(listenNewClientsToken.Token);
                    // set up the new client
                    client.NoDelay = true;
                    // create a connection object to hold the client
                    Connection conn = new Connection(this, client);
                    connections.Add(conn);
                    Statistics.ConnectionCount = connections.Count;
                    ConsoleManager.Instance.WriteLine($"Client connected {conn.Client.Client.RemoteEndPoint.ToString()}", MsgType.ServerInfo);
                    OnConnectionAccepted(new ConnectionEventArgs(conn));
                    conn.Listening = true;
                    // begin listening to the new connection
                    new Thread(() => HandleClient(conn)).Start();
                }
                catch (Exception e) { ConsoleManager.Instance.WriteLine($"Error accepting client, {e.Message}", MsgType.ServerWarning); }
            }
        }

        private async void HandleClient(Connection conn)
        {
            conn.Listening = true;
            NetworkStream stream = conn.Client.GetStream();
            bool bufferInUse = false;
            byte[] buffer = new byte[conn.Client.ReceiveBufferSize];
            byte[] packetData = null;
            int packetBytesRead = 0;

            while (conn.Listening && conn.Client != null && conn.Client.Connected)
            {
                try
                {
                    int bytesRead = 0;
                    // check if we should start building a new packet
                    if (packetData == null)
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        bufferInUse = true;
                        Statistics.BytesRecieved += bytesRead;
                        // get the length of the next packet in the stream
                        int packetLength = BitConverter.ToInt32(buffer, 4) + 8; // 4 byte header, 4 byte length, n byte payload. Add 8 to include header and length
                        packetData = new byte[packetLength]; // make a buffer for the packet
                    }
                    // check if we have a packet to build
                    if (packetData != null)
                    {
                        // build the packet
                        while (packetBytesRead < packetData.Length)
                        {
                            // check if the buffer contains new data
                            if (!bufferInUse)
                            {
                                bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(buffer.Length, packetData.Length - packetBytesRead));
                                bufferInUse = true;
                            }
                            // copy the packet data from the buffer to the packet data array
                            Buffer.BlockCopy(buffer, 0, packetData, packetBytesRead, Math.Min(bytesRead, packetData.Length));
                            bufferInUse = false;
                            packetBytesRead += bytesRead;
                        }
                        if (packetBytesRead >= packetData.Length)
                        {
                            Packet p = new Packet(packetData, conn); // parse the packet, as a client we know the server sent it to us
                            Statistics.PacketsRecieved++;
                            packetData = null;
                            packetBytesRead = 0;
                            //ConsoleManager.Instance.WriteLine($"Constructed a packet of {packetLength} bytes, {bytesLeft} bytes remaining", MsgType.Info);
                            OnPacketRecieved(new PacketRecievedEventArgs(p));
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleManager.Instance.WriteLine($"Error recieving data from {conn.Address}, {e.Message}", MsgType.ServerWarning);
                }
            }
        }

        /// <summary>
        /// Dissconnects the given client from the server
        /// </summary>
        /// <param name="conn"></param>
        public void DissconnectClient(Connection conn)
        {
            ConsoleManager.Instance.WriteLine($"Client {conn.Address} dissconnected", MsgType.ServerInfo);
            conn.Listening = false;
            conn.Client.Close();
            connections.Remove(conn);
            Statistics.ConnectionCount = connections.Count;
            OnConnectionLost(new ConnectionEventArgs(conn));
        }

        /// <summary>
        /// Sends a packet to the given connection
        /// </summary>
        /// <param name="p">packet to send</param>
        /// <param name="c">connection to send to</param>
        public void SendData(Packet p, Connection c)
        {
            if (p == null)
                return;
            if (c == null)
                return;
            try
            {
                if (c!= null && !c.Client.Connected)
                    return;
                NetworkStream stream = c.Client.GetStream();
                stream.WriteAsync(p.Data, 0, p.Data.Length);
                Statistics.PacketsSent++;
                Statistics.BytesSent += p.Data.Length;
            }
            catch (Exception)
            {
                c.Close();
            }
            //ConsoleManager.Instance.WriteLine($"Sent {p.Data.Length} bytes to {c.Client.Client.RemoteEndPoint}", MsgType.ServerInfo);
        }

        /// <summary>
        /// Sends a packet to all the current connections
        /// </summary>
        /// <param name="p">packet to send</param>
        public void SendDataToAll(Packet p)
        {
            if (p == null)
                return;

            foreach (Connection c in connections.ToArray())
            {
                if (c != null && c.Client.Connected)
                    SendData(p, c);
            }
        }
    }
}
