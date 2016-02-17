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
        public int RecieveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public int PacketsRecieved { get; set; }
        public int PacketsSent { get; set; }
        public int BytesRecieved { get; set; }
        public int BytesSent { get; set; }
        public int ServerPort { get; set; }
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
        public static TcpState GetState(this TcpClient tcpClient)
        {
            TcpConnectionInformation tcpconninfo = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return tcpconninfo != null ? tcpconninfo.State : TcpState.Unknown;
        }
    }

    public class NetClient : IEngineComponent
    {
        public Engine Engine { get; }

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

        public NetClient(Engine e)
        {
            Engine = e;
            client = new TcpClient();
            client.NoDelay = true;
            client.ReceiveBufferSize = 1048576;
            connecting = false;
            Listening = false;

            Statistics = new NetClientStats();
            Statistics.RecieveBufferSize = client.ReceiveBufferSize;
        }

        public async Task<bool> ConnectAsync(string address, int port)
        {
            if (connecting)
                throw new MethodAccessException("Cannot call NetClient.ConnectAsync() while it is already connected");
            connecting = true;

            try
            {
                Engine.Console.WriteLine($"Attempting to connect to {address}:{port}", MsgType.Info);
                await client.ConnectAsync(address, port);
                Engine.Console.WriteLine($"Connected to {address}:{port}", MsgType.Info);
                Statistics.ServerPort = port;
                Statistics.ServerAddress = address;
                return true;
            }
            catch (SocketException e)
            {
                Engine.Console.WriteLine($"Unable to reach {address}:{port}. Error: {e.Message}", MsgType.Info);
                return false;
            }
        }

        public bool Connect(string address, int port)
        {
            if (connecting)
                throw new MethodAccessException("Cannot call NetClient.ConnectAsync() while it is already connected");
            connecting = true;

            try
            {
                Engine.Console.WriteLine($"Attempting to connect to {address}:{port}", MsgType.Info);
                client.Connect(address, port);
                Engine.Console.WriteLine($"Connected to {address}:{port}", MsgType.Info);
                Statistics.ServerPort = port;
                Statistics.ServerAddress = address;
                return true;
            }
            catch (SocketException e)
            {
                Engine.Console.WriteLine($"Unable to reach {address}:{port}. Error: {e.Message}", MsgType.Info);
                return false;
            }
        }

        public void Dissconnect()
        {
            Listening = false;
            client.Close();
        }

        public async void SendData(Packet p)
        {
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(p.Data, 0, p.Data.Length);
            Statistics.PacketsSent++;
            Statistics.BytesSent += p.Data.Length;

            //Engine.Console.WriteLine($"Sent {p.Data.Length} bytes to {client.Client.RemoteEndPoint}", MsgType.ServerInfo);
        }

        public async void BeginListenAsync()
        {
            Listening = true;
            byte[] buffer = new byte[client.ReceiveBufferSize];
            NetworkStream stream = client.GetStream();
            while (Listening && client.Connected)
            {
                try
                {
                    int bytesLeft = await stream.ReadAsync(buffer, 0, buffer.Length);
                    Statistics.BytesRecieved += bytesLeft;
                    //Engine.Console.WriteLine($"Recieved {bytesLeft} bytes from server", MsgType.Info);
                    int packetLength;
                    int offset = 0;
                    do
                    {
                        packetLength = BitConverter.ToInt32(buffer, offset + 4) + 8;
                        byte[] data = new byte[packetLength];
                        Buffer.BlockCopy(buffer, offset, data, 0, packetLength);
                        offset = packetLength;
                        bytesLeft -= packetLength;
                        Packet p = new Packet(data, null);
                        Statistics.PacketsRecieved++;
                        //Engine.Console.WriteLine($"Constructed a packet of {packetLength} bytes, {bytesLeft} bytes remaining", MsgType.Info);
                        OnPacketRecieved(new PacketRecievedEventArgs(p));
                    } while (bytesLeft > 0);
                }
                catch (Exception e) { Engine.Console.WriteLine($"Error recieving data, {e.Message}", MsgType.Warning); }
            }
            OnLostConnection(EventArgs.Empty);
        }
    }
}
