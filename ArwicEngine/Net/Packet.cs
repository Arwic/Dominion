using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace ArwicEngine.Net
{
    public class Connection
    {
        public bool Listening { get; set; }
        public NetServer Server { get; }
        public TcpClient Client { get; }
        public string Address
        {
            get
            {
                try
                {
                    return Client.Client.RemoteEndPoint.ToString();
                }
                catch (Exception)
                {
                    return "NULL";
                }
            }
        }
        public Connection(NetServer server, TcpClient client)
        {
            Server = server;
            Client = client;
        }

        /// <summary>
        /// Closes the connection after a given time
        /// </summary>
        /// <param name="time"></param>
        public async void Close(int time)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(time);
                Close();
            });
        }

        /// <summary>
        /// Immediatley closes the connection
        /// </summary>
        public void Close()
        {
            Listening = false;
            Client.Close();
        }
    }

    public class Packet
    {
        public int Header { get; private set; }
        public object Item => Items[0];
        public List<object> Items { get; private set; }
        public Connection Sender { get; }
        public byte[] Data { get; private set; }

        public Packet(int header, params object[] item)
        {
            Items = new List<object>();
            foreach (object obj in item)
                Items.Add(obj);
            Header = header;
            Serialize();
        }

        public Packet(byte[] data, Connection sender)
        {
            Sender = sender;
            Data = data;
            Deserialize();
        }

        private void Serialize()
        {
            List<byte> fullData = new List<byte>();
            List<byte> itemData = new List<byte>();
            int itemLength = 0;
            if (Items != null)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    formatter.Serialize(ms, Items);
                    itemData.AddRange(ms.ToArray());
                }
                itemLength = itemData.Count;
            }

            fullData.AddRange(BitConverter.GetBytes(Header)); // 4 byte header
            fullData.AddRange(BitConverter.GetBytes(itemLength)); // 4 byte length
            fullData.AddRange(itemData); // 'itemLen' byte length

            Data = fullData.ToArray();
        }

        private void Deserialize()
        {
            if (Data != null && !Data.All(singleByte => singleByte == 0))
            {
                Header = BitConverter.ToInt32(Data, 0);
                int itemLength = BitConverter.ToInt32(Data, 4);
                if (itemLength == 0)
                    return;
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(Data, 8, itemLength))
                {
                    Items = (List<object>)formatter.Deserialize(ms);
                }
            }
        }
    }
}
