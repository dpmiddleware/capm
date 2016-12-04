using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebRunner.Services;
using WebRunner.Models;

namespace WebRunner.Controllers
{
    public class AipsController : ApiController
    {
        private IAipStore _store;

        public AipsController(IAipStore store)
        {
            _store = store;
            
        }

        [HttpPost]
        public async Task Post()
        {
            await _store.Store(new Aip()
            {
                ContentType = Request.Content.Headers.ContentType.ToString(),
                Bytes = await Request.Content.ReadAsByteArrayAsync()
            });
        }

        [HttpGet]
        public Task<string[]> Get()
        {
            return _store.GetAllStoredIds();
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string id)
        {
            if (!await _store.Exists(id))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
            var aip = await _store.Get(id);
            var content = new ByteArrayContent(aip.Bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(aip.ContentType);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = content;
            return response;
        }
    }
}