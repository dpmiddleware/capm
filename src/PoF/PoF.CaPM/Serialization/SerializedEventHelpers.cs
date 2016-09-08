using Newtonsoft.Json;
using PoF.CaPM.IngestSaga;
using PoF.CaPM.IngestSaga.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.Serialization
{
    public static class SerializedEventHelpers
    {
        private static readonly JsonSerializer _eventSerializer = new JsonSerializer();

        public static Type GetEventType(this SerializedEvent serializedEvent)
        {
            return Assembly.GetExecutingAssembly().GetType(serializedEvent.EventTypeName);
        }

        public static IIngestEvent GetEventObject(this SerializedEvent serializedEvent)
        {
            var type = serializedEvent.GetEventType();
            using (var reader = new StringReader(serializedEvent.SerializedEventData))
            {
                return (IIngestEvent)_eventSerializer.Deserialize(reader, type);
            }
        }

        public static SerializedEvent ToSerializedEvent(this IIngestEvent eventObject)
        {
            var type = eventObject.GetType();
            using (var writer = new StringWriter())
            {
                _eventSerializer.Serialize(writer, eventObject);
                var serializedEventData = writer.GetStringBuilder().ToString();
                return new SerializedEvent()
                {
                    EventTypeName = type.FullName,
                    SerializedEventData = serializedEventData
                };
            }
        }

        public static Stream GetSerializedStream(this SerializedEvent serializedEvent)
        {
            var stream = new MemoryStream();
            var jsonWriter = new StreamWriter(stream);
            _eventSerializer.Serialize(jsonWriter, serializedEvent);
            jsonWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static IIngestEvent ReadSerializedEvent(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return _eventSerializer.Deserialize<SerializedEvent>(jsonReader).GetEventObject();
            }
        }
    }
}
