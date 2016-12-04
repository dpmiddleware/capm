using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebRunner.Models;

namespace WebRunner.Services
{
    public class InMemoryAipStore : IAipStore
    {
        private static Dictionary<string, Aip> _storedPackages = new Dictionary<string, Aip>();
        public Task<bool> Exists(string id)
        {
            return Task.FromResult(_storedPackages.ContainsKey(id));
        }

        public Task<Aip> Get(string id)
        {
            if (_storedPackages.ContainsKey(id))
            {
                return Task.FromResult(_storedPackages[id]);
            }
            else
            {
                return Task.FromException<Aip>(new Exception($"There is no package with the ID '{id}'"));
            }
        }

        public Task<string[]> GetAllStoredIds()
        {
            return Task.FromResult(_storedPackages.Keys.ToArray());
        }

        public Task<string> Store(Aip aip)
        {
            var id = Guid.NewGuid().ToString();
            _storedPackages.Add(id, aip);
            return Task.FromResult(id);
        }
    }
}