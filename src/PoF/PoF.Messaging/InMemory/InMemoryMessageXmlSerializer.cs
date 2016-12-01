using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PoF.Messaging.InMemory
{
    internal class InMemoryMessageXmlSerializer
    {
        private InMemoryMessageXmlSerializer()
        {

        }

        public static InMemoryMessageXmlSerializer Instance { get; } = new InMemoryMessageXmlSerializer();

        private Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();
        private object _serializerCreationLockObject = new object();
        private XmlSerializer GetSerializer<T>()
        {
            var type = typeof(T);
            if (!_serializers.ContainsKey(type))
            {
                lock (_serializerCreationLockObject)
                {
                    if (!_serializers.ContainsKey(type))
                    {
                        _serializers[type] = new XmlSerializer(type);
                    }
                }
            }
            return _serializers[type];
        }

        public string Serialize<T>(T message)
        {
            var serializer = GetSerializer<T>();
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, message);
                writer.Flush();
                var messageString = writer.GetStringBuilder().ToString();
                return messageString;
            }
        }

        public bool TryDeserialize<T>(string message, out T output)
        {
            var serializer = GetSerializer<T>();
            try
            {
                using (var reader = new StringReader(message))
                {
                    T obj = (T)serializer.Deserialize(reader);
                    output = obj;
                    return true;
                }
            }
            catch
            {
                output = default(T);
                return false;
            }
        }
    }
}
