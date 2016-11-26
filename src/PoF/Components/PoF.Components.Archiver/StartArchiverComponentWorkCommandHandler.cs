using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PoF.Components.Archiver
{
    internal class StartArchiverComponentWorkCommandHandler : ICommandHandler<StartComponentWorkCommand>
    {
        private const string DownloadedFileKey_Bytes = "downloadedfile-bytes";
        private const string DownloadedFileKey_ContentType = "downloadedfile-contenttype";
        private IStagingStoreContainer _stagingStoreContainer;
        private IMessageSenderFactory _messageSenderFactory;

        public StartArchiverComponentWorkCommandHandler(IStagingStoreContainer stagingStoreContainer, IMessageSenderFactory messageSenderFactory)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentWorkCommand command)
        {
            try
            {
                var store = await _stagingStoreContainer.GetSharedStore(command.IngestId);
                await EnsureAllNeededItemsExistInStore(store);
                var client = new HttpClient();
                var byteStream = await store.GetItemAsync(DownloadedFileKey_Bytes);
                var contentType = await store.GetItemAsync<string>(DownloadedFileKey_ContentType);
                var uri = command.ComponentSettings;
                var aip = new StreamContent(byteStream);
                aip.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                var response = await client.PostAsync(uri, aip).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
                    {
                        ComponentExecutionId = command.ComponentExecutionId,
                        IngestId = command.IngestId
                    });
                }
                else
                {
                    throw new Exception($"Failed sending AIP to DPS. Response status code: {response.StatusCode} ({response.ReasonPhrase})");
                }
            }
            catch (Exception e)
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = e.ToString()
                });
            }
        }

        private static async Task EnsureAllNeededItemsExistInStore(IComponentStagingStore store)
        {
            var missingKeys = new List<string>();
            foreach(var key in new string[] { DownloadedFileKey_Bytes, DownloadedFileKey_ContentType })
            {
                if (!await store.HasItemAsync(key))
                {
                    missingKeys.Add(key);
                }
            }
            if (missingKeys.Any())
            {
                throw new Exception("Could not send file to archive since there were missing mandator keys in the store. The missing keys were: " + string.Join(", ", missingKeys));
            }
        }
    }
}