// Dominion - Copyright (C) Timothy Ings
// NetClient.cs
// This file contains classes that define net client

using ArwicEngine.Core;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
            client.ReceiveBufferSize = 1048576;
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
            byte[] buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = client.GetStream();
            while (Listening && client.Connected)
            {
                try
                {
                    // await some data
                    int bytesLeft = await stream.ReadAsync(buffer, 0, buffer.Length);
                    Statistics.BytesRecieved += bytesLeft;
                    //Engine.Console.WriteLine($"Recieved {bytesLeft} bytes from server", MsgType.Info);
                    int packetLength;
                    int offset = 0;
                    do
                    {
                        packetLength = BitConverter.ToInt32(buffer, offset + 4) + 8; // get the length of the next packet in the stream
                        byte[] data = new byte[packetLength]; // read only the next packet's data from the stream
                        Buffer.BlockCopy(buffer, offset, data, 0, packetLength); // copy the packet data from the buffer to the data array
                        offset = packetLength; // update the offset as we have already parsed the packet(s) before it
                        bytesLeft -= packetLength; // update the number of bytes left to parse
                        Packet p = new Packet(data, null); // parse the packet, as a client we know the server sent it to us
                        Statistics.PacketsRecieved++;
                        //Engine.Console.WriteLine($"Constructed a packet of {packetLength} bytes, {bytesLeft} bytes remaining", MsgType.Info);
                        OnPacketRecieved(new PacketRecievedEventArgs(p));
                    } while (bytesLeft > 0); // keep parsing packets from the buffer until there are none left
                }
                catch (Exception e) { ConsoleManager.Instance.WriteLine($"Error recieving data, {e.Message}", MsgType.Warning); }
            }
            OnLostConnection(EventArgs.Empty);
        }
    }
}
