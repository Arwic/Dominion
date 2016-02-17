using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace ArwicEngine.Core
{
    public static class SerializationHelper
    {
        public static void XmlSerialize<T>(string file, object obj)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                xmls.Serialize(fs, obj);
            }
        }

        public static T XmlDeserialize<T>(string file)
        {
            XmlSerializer serializer = null;
            serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StreamReader(file));
        }

        public static byte[] BinarySerialize(object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T BinaryDeserialize<T>(byte data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
