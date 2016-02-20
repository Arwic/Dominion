// Dominion - Copyright (C) Timothy Ings
// Packet.cs
// This file contains classes that define a packet and network connections

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
        /// <summary>
        /// Gets ot sets a value indicating whether the connection is listening for data
        /// </summary>
        public bool Listening { get; set; }

        /// <summary>
        /// Gets the net server this connection is associated with
        /// </summary>
        public NetServer Server { get; }

        /// <summary>
        /// Gets the tcp client this connection uses
        /// </summary>
        public TcpClient Client { get; }

        /// <summary>
        /// Gets the address of the connetion
        /// </summary>
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

        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
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
        /// <summary>
        /// Gets an int that acts as the header of the packet
        /// This should be used to differenciate the contents of packets
        /// </summary>
        public int Header { get; private set; }

        /// <summary>
        /// Gets the first or only object in the packet
        /// </summary>
        public object Item => Items[0];

        /// <summary>
        /// Gets a list of objects in the packet
        /// </summary>
        public List<object> Items { get; private set; }

        /// <summary>
        /// Gets the sender's connection
        /// This is null when the packet is parsed by a client
        /// </summary>
        public Connection Sender { get; }

        /// <summary>
        /// The raw data in the packet
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Creates a packet wit the given header and the given items, the items are immediately serialized
        /// </summary>
        /// <param name="header"></param>
        /// <param name="item"></param>
        public Packet(int header, params object[] item)
        {
            Items = new List<object>();
            foreach (object obj in item)
                Items.Add(obj);
            Header = header;
            Serialize();
        }

        /// <summary>
        /// Creates a packet based on the given data
        /// The data is immediately deserialized into a list of objects
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
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
                    formatter.Serialize(ms, Items); // Serialize the list of objects
                    itemData.AddRange(ms.ToArray()); // add the result of binary serialization to the item data buffer
                }
                itemLength = itemData.Count; // get the length of the item
            }

            fullData.AddRange(BitConverter.GetBytes(Header)); // 4 byte header
            fullData.AddRange(BitConverter.GetBytes(itemLength)); // 4 byte length
            fullData.AddRange(itemData); // 'itemLen' byte length

            Data = fullData.ToArray(); // set the packets Data property to the fully formatted data
        }

        private void Deserialize()
        {
            // check if the Data is all zeros
            if (Data != null && !Data.All(singleByte => singleByte == 0))
            {
                Header = BitConverter.ToInt32(Data, 0); // 4 byte header
                int itemLength = BitConverter.ToInt32(Data, 4); // 4 bytes packet length
                if (itemLength == 0) // if there is no item data, dont try deserializing it
                    return;
                BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream(Data, 8, itemLength)) // create a memory stream with correct offset
                {
                    Items = (List<object>)formatter.Deserialize(ms); // deserialize the object list and assign it to the items property
                }
            }
        }
    }
}
