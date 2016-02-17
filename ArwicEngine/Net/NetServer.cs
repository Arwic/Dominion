using ArwicEngine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArwicEngine.Net
{
    public class NetServerStats
    {
        public int RecieveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public int PacketsRecieved { get; set; }
        public int PacketsSent { get; set; }
        public int BytesRecieved { get; set; }
        public int BytesSent { get; set; }
        public int Port { get; set; }
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

    public class NetServer : IEngineComponent
    {
        public Engine Engine { get; }

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
        /// Occurs when the server looses connection to a client
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

        public NetServer(Engine e)
        {
            Engine = e;
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

            Engine.Console.WriteLine($"Starting server on {port}", MsgType.ServerInfo);
            listener = new TcpListener(IPAddress.Any, port);
            Statistics.RecieveBufferSize = listener.Server.ReceiveBufferSize;
            Statistics.SendBufferSize = listener.Server.SendBufferSize;
            listener.Start(100);
            Engine.Console.WriteLine($"Listening on {port}", MsgType.ServerInfo);

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
                    TcpClient client = await listener.AcceptTcpClientAsync().WithCancellation(listenNewClientsToken.Token);
                    client.NoDelay = true;
                    Connection conn = new Connection(this, client);
                    connections.Add(conn);
                    Statistics.ConnectionCount = connections.Count;
                    Engine.Console.WriteLine($"Client connected {conn.Client.Client.RemoteEndPoint.ToString()}", MsgType.ServerInfo);
                    OnConnectionAccepted(new ConnectionEventArgs(conn));
                    conn.Listening = true;
                    new Thread(() => HandleClient(conn)).Start();
                }
                catch (Exception e) { Engine.Console.WriteLine($"Error accepting client, {e.Message}", MsgType.ServerWarning); }
            }
        }

        private async void HandleClient(Connection conn)
        {
            conn.Listening = true;
            byte[] buffer = new byte[conn.Client.ReceiveBufferSize];
            NetworkStream stream = conn.Client.GetStream();
            while (conn.Listening)
            {
                if (conn.Client != null && conn.Client.Connected)
                {
                    try
                    {
                        int bytesLeft = await stream.ReadAsync(buffer, 0, buffer.Length);
                        Statistics.BytesRecieved += bytesLeft;
                        //Engine.Console.WriteLine($"Recieved {bytesLeft} bytes from {conn.Address}", MsgType.ServerInfo);
                        if (bytesLeft == 0)
                            DissconnectClient(conn);
                        else
                        {
                            int packetLength;
                            int offset = 0;
                            do
                            {
                                packetLength = BitConverter.ToInt32(buffer, offset + 4) + 8;
                                byte[] data = new byte[packetLength];
                                Buffer.BlockCopy(buffer, offset, data, 0, packetLength);
                                offset = packetLength;
                                bytesLeft -= packetLength;
                                Packet p = new Packet(data, conn);
                                //Engine.Console.WriteLine($"Constructed a packet of {packetLength} bytes, {bytesLeft} bytes remaining", MsgType.ServerInfo);
                                Statistics.PacketsRecieved++;
                                OnPacketRecieved(new PacketRecievedEventArgs(p));
                            } while (bytesLeft > 0);
                        }
                    }
                    catch (Exception e)
                    {
                        Engine.Console.WriteLine($"Error recieving data from {conn.Address}, {e.Message}", MsgType.ServerWarning);
                    }
                }
            }
        }

        private void DissconnectClient(Connection conn)
        {
            Engine.Console.WriteLine($"Client {conn.Address} dissconnected", MsgType.ServerInfo);
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
            //Engine.Console.WriteLine($"Sent {p.Data.Length} bytes to {c.Client.Client.RemoteEndPoint}", MsgType.ServerInfo);
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
