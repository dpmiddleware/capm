using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebRunner.Models;

namespace WebRunner.Services
{
    public interface IAipStore
    {
        Task<bool> Exists(string id);
        Task<string> Store(Aip aip);
        Task<Aip> Get(string id);
        Task<string[]> GetAllStoredIds();
    }
}