using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WebRunner.Services;
using WebRunner.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace WebRunner.Controllers
{
    [Route("api/aips")]
    public class AipsController : ControllerBase
    {
        private IAipStore _store;

        public AipsController(IAipStore store)
        {
            _store = store;
        }

        [HttpPost]
        public async Task Post()
        {
            using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream).ConfigureAwait(false);
            var newId = await _store.Store(new Aip()
            {
                ContentType = Request.ContentType.ToString(),
                Bytes = memoryStream.ToArray(),
                Timestamp = DateTimeOffset.UtcNow
            });
            await PreservationSystemHub.OnNewAip(newId).ConfigureAwait(false);
        }

        [HttpGet]
        public Task<string[]> Get()
        {
            return _store.GetAllStoredIds();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (!await _store.Exists(id))
            {
                return NotFound();
            }
            var aip = await _store.Get(id);
            return File(aip.Bytes, aip.ContentType);
        }
    }
}