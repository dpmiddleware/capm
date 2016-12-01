using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PoF.StagingStore.InMemory
{
    internal class InMemoryComponentStagingStore : IComponentStagingStore
    {
        private class LockableStream
        {
            public LockableStream(Stream stream)
            {
                this.Stream = stream;
                this.Lock = new Mutex(initiallyOwned: false);
            }

            public Mutex Lock { get; private set; }
            public Stream Stream { get; private set; }
        }

        private Dictionary<string, LockableStream> _streams = new Dictionary<string, LockableStream>();
        private static readonly Random _randomizer = new Random();

        public async Task<Stream> GetItemAsync(string identifier)
        {
            EnsureIdentifierIsValidFilename(identifier);
            //Pretend this takes a little bit of time, to emulate a slower persistent store
            await Task.Delay((int)(_randomizer.NextDouble() * 600)).ConfigureAwait(false);

            var stream = _streams[identifier];
            try
            {
                stream.Lock.WaitOne();
                stream.Stream.Seek(0, SeekOrigin.Begin);
                var memoryStream = new MemoryStream();
                await stream.Stream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            finally
            {
                stream.Lock.ReleaseMutex();
            }
        }

        public Task<bool> HasItemAsync(string identifier)
        {
            EnsureIdentifierIsValidFilename(identifier);
            return Task.FromResult(_streams.ContainsKey(identifier));
        }

        public async Task SetItemAsync(string identifier, Stream stream)
        {
            EnsureIdentifierIsValidFilename(identifier);
            //Pretend this takes a little bit of time, to emulate a slower persistent store
            await Task.Delay((int)(_randomizer.NextDouble() * 600)).ConfigureAwait(false);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _streams[identifier] = new LockableStream(memoryStream);
        }

        private void EnsureIdentifierIsValidFilename(string identifier)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars().Where(c => identifier.Contains(c)).ToArray();
            if (invalidCharacters.Any())
            {
                throw new ArgumentException($"The identifier '{identifier}' contains invalid character. The following invalid characters were in the identifier: {string.Join(", ", invalidCharacters)}");
            }
        }

        public Task<string[]> GetAvailableIdentifiersAsync()
        {
            return Task.FromResult(_streams.Keys.ToArray());
        }

        public Task RemoveItemAsync(string identifier)
        {
            _streams.Remove(identifier);
            return Task.CompletedTask;
        }
    }
}