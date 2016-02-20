// Dominion - Copyright (C) Timothy Ings
// SerializationHelper.cs
// This file defines utility methods that provide xml and binary serialization

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace ArwicEngine.Core
{
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes the given object into xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <param name="obj"></param>
        public static void XmlSerialize<T>(string file, object obj)
        {
            XmlSerializer xmls = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(file, FileMode.Create))
            {
                xmls.Serialize(fs, obj);
            }
        }

        /// <summary>
        /// Deserializes the given xml file into the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string file)
        {
            XmlSerializer serializer = null;
            serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(new StreamReader(file));
        }

        /// <summary>
        /// Serializes the given object into binary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] BinarySerialize(object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Deserializes the given bytes into the given object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
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
