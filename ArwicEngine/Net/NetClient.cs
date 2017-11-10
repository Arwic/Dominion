// Dominion - Copyright (C) Timothy Ings
// NetClient.cs
// This file contains classes that define net client

using ArwicEngine.Core;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArwicEngine.Net
{
    public class NetClientStats
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
        /// Gets the port of the net server the net client is currently connected to
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Gets the address of the net server the net client is currently connected to
        /// </summary>
        public string ServerAddress { get; set; }
    }

    public class PacketRecievedEventArgs : EventArgs
    {
        public Connection Sender => Packet.Sender;
        public Packet Packet { get; }

        public PacketRecievedEventArgs(Packet p)
        {
            Packet = p;
        }
    }

    public static class TcpClientExtension
    {
        /// <summary>
        /// Returns the state of the given tcp client
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public static TcpState GetState(this TcpClient tcpClient)
        {
            TcpConnectionInformation tcpconninfo = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return tcpconninfo != null ? tcpconninfo.State : TcpState.Unknown;
        }
    }

    public class NetClient
    {
        /// <summary>
        /// Gets a value indicating whether the client is connected to a server
        /// </summary>
        public bool Connected => client.GetState() == TcpState.Established;

        /// <summary>
        /// Gets a value indicating whether the client is listening
        /// </summary>
        public bool Listening { get; set; }

        public NetClientStats Statistics { get; private set; }

        private TcpClient client;
        private bool connecting;

        #region Events
        /// <summary>
        /// Occurs when the client recieves a packet
        /// </summary>
        public event EventHandler<PacketRecievedEventArgs> PacketRecieved;
        /// <summary>
        /// Occurs when the client looses connection to the server
        /// </summary>
        public event EventHandler<EventArgs> LostConnection;
        #endregion

        #region Event Handlers
        protected virtual void OnPacketRecieved(PacketRecievedEventArgs args)
        {
            if (PacketRecieved != null)
                PacketRecieved(this, args);
        }
        protected virtual void OnLostConnection(EventArgs args)
        {
            if (LostConnection != null)
                LostConnection(this, args);
        }
        #endregion

        /// <summary>
        /// Creates a new net client
        /// </summary>
        public NetClient()
        {
            // set up values
            client = new TcpClient();
            client.NoDelay = true;
            connecting = false;
            Listening = false;

            Statistics = new NetClientStats();
            Statistics.RecieveBufferSize = client.ReceiveBufferSize;
        }

        /// <summary>
        /// Asynchronously attempts to connect to a net server with the given address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(string address, int port)
        {
            if (connecting)
                throw new MethodAccessException("Cannot call NetClient.ConnectAsync() while already connected");
            connecting = true;

            try
            {
                ConsoleManager.Instance.WriteLine($"Attempting to connect to {address}:{port}", MsgType.Info);
                await client.ConnectAsync(address, port);
                ConsoleManager.Instance.WriteLine($"Connected to {address}:{port}", MsgType.Info);
                Statistics.ServerPort = port;
                Statistics.ServerAddress = address;
                return true;
            }
            catch (SocketException e)
            {
                ConsoleManager.Instance.WriteLine($"Unable to reach {address}:{port}. Error: {e.Message}", MsgType.Info);
                return false;
            }
        }

        /// <summary>
        /// Attempts the connect to a net server with the given address and port
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string address, int port)
        {
            if (connecting)
                throw new MethodAccessException("Cannot call NetClient.ConnectAsync() while already connected");
            connecting = true;

            try
            {
                ConsoleManager.Instance.WriteLine($"Attempting to connect to {address}:{port}", MsgType.Info);
                client.Connect(address, port);
                ConsoleManager.Instance.WriteLine($"Connected to {address}:{port}", MsgType.Info);
                Statistics.ServerPort = port;
                Statistics.ServerAddress = address;
                return true;
            }
            catch (SocketException e)
            {
                ConsoleManager.Instance.WriteLine($"Unable to reach {address}:{port}. Error: {e.Message}", MsgType.Info);
                return false;
            }
        }

        /// <summary>
        /// Dissconnects from the currently connected server
        /// </summary>
        public void Dissconnect()
        {
            Listening = false;
            client.Close();
        }

        /// <summary>
        /// Sets the given packet to the server
        /// </summary>
        /// <param name="p"></param>
        public async void SendData(Packet p)
        {
            await Task.Delay(30); // FIXME add more robust flow control
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(p.Data, 0, p.Data.Length);
            Statistics.PacketsSent++;
            Statistics.BytesSent += p.Data.Length;

            //Engine.Console.WriteLine($"Sent {p.Data.Length} bytes to {client.Client.RemoteEndPoint}", MsgType.ServerInfo);
        }

        /// <summary>
        /// Begins listening for packets from the server
        /// </summary>
        public async void BeginListenAsync()
        {
            Listening = true;
            NetworkStream stream = client.GetStream();
            bool bufferInUse = false;
            byte[] buffer = new byte[client.ReceiveBufferSize];
            byte[] packetData = null;
            int packetBytesRead = 0;

            while (Listening && client.Connected)
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
                            Packet p = new Packet(packetData, null); // parse the packet, as a client we know the server sent it to us
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
                    ConsoleManager.Instance.WriteLine($"Error recieving data, {e.Message}", MsgType.Warning);
                }
            }
            OnLostConnection(EventArgs.Empty);
        }
    }
}
