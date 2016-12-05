using PoF.CaPM.IngestSaga.Events;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoF.CaPM.Serialization;
using PoF.Messaging;

namespace PoF.CaPM.IngestSaga
{
    internal class CaPMIngestEventStore
    {
        private const string CaPMComponentStagingStoreID = "CaPM";
        private IComponentStagingStore _componentStagingStore;
        private Guid _ingestId;

        private CaPMIngestEventStore(IComponentStagingStore componentStagingStore, Guid ingestId)
        {
            this._componentStagingStore = componentStagingStore;
            this._ingestId = ingestId;
        }

        public async Task StoreEvent(IIngestEvent eventObject, IMessageSender<SerializedEvent> ingestEventSender)
        {
            eventObject.IngestId = _ingestId;
            eventObject.Timestamp = GetTimestamp();
            eventObject.Order = await GetNumberOfStoredEvents() + 1;
            var serializedEvent = eventObject.ToSerializedEvent();
#if DEBUG
            System.Diagnostics.Debug.WriteLine(_debug_SerializeToJsonString(serializedEvent));
#endif
            await _componentStagingStore.SetItemAsync(Guid.NewGuid().ToString(), serializedEvent.GetSerializedStream()).ConfigureAwait(false);
            await ingestEventSender.Send(eventObject.ToSerializedEvent()).ConfigureAwait(false);
        }

#if DEBUG
        private static readonly Newtonsoft.Json.JsonSerializer _debug_serializeToJsonStringSerializer = new Newtonsoft.Json.JsonSerializer();

        public string _debug_SerializeToJsonString(SerializedEvent serializedEvent)
        {
            using (var writer = new System.IO.StringWriter())
            {
                using (var jsonWriter = new Newtonsoft.Json.JsonTextWriter(writer))
                {
                    _debug_serializeToJsonStringSerializer.Serialize(jsonWriter, serializedEvent);
                    return writer.GetStringBuilder().ToString();
                }
            }
        }
#endif

        public async Task<IIngestEvent[]> GetStoredEvents()
        {
            var identifiers = await _componentStagingStore.GetAvailableIdentifiersAsync();
            var events = await Task.WhenAll(identifiers.Select(async id =>
            {
                var item = await _componentStagingStore.GetItemAsync(id).ConfigureAwait(false);
                return item.ReadSerializedEvent();
            })).ConfigureAwait(false);
            return events.OrderBy(e => e.Timestamp).ToArray();
        }

        public async Task<uint> GetNumberOfStoredEvents()
        {
            var identifiers = await _componentStagingStore.GetAvailableIdentifiersAsync().ConfigureAwait(false);
            return (uint)identifiers.Count();
        }

        private DateTimeOffset GetTimestamp()
        {
            return DateTimeOffset.UtcNow;
        }

        public static async Task<CaPMIngestEventStore> CreateCaPMEventStore(IStagingStoreContainer stagingStoreContainer, Guid ingestId)
        {
            await stagingStoreContainer.CreateStoreForContextIdAsync(ingestId).ConfigureAwait(false);
            return await GetCaPMEventStore(stagingStoreContainer, ingestId).ConfigureAwait(false);
        }

        public static async Task<CaPMIngestEventStore> GetCaPMEventStore(IStagingStoreContainer stagingStoreContainer, Guid ingestId)
        {
            var stagingStore = await stagingStoreContainer.GetStoreForContextIdAsync(ingestId).ConfigureAwait(false);
            var capmComponentStore = await stagingStore.GetComponentStagingStoreAsync(CaPMComponentStagingStoreID).ConfigureAwait(false);
            return new CaPMIngestEventStore(capmComponentStore, ingestId);
        }
    }
}
