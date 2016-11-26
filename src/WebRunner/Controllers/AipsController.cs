using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace WebRunner.Controllers
{
    public class AipsController : ApiController
    {
        private static readonly List<Aip> _storedPackages = new List<Aip>();

        [HttpPost]
        public async Task Post()
        {
            _storedPackages.Add(new Controllers.AipsController.Aip()
            {
                ContentType = Request.Content.Headers.ContentType.ToString(),
                Bytes = await Request.Content.ReadAsByteArrayAsync()
            });
        }

        [HttpGet]
        public int[] Get()
        {
            return Enumerable.Range(0, _storedPackages.Count).ToArray();
        }

        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            if (id < 0 || id >= _storedPackages.Count)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }
            var aip = _storedPackages[id];
            var content = new ByteArrayContent(aip.Bytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(aip.ContentType);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = content;
            return response;
        }

        public class Aip
        {
            public string ContentType { get; set; }
            public byte[] Bytes { get; set; }
        }
    }
}