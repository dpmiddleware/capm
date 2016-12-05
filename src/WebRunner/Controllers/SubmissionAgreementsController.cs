using PoF.CaPM.SubmissionAgreements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebRunner.Controllers
{
    public class SubmissionAgreementsController : ApiController
    {
        private ISubmissionAgreementStore _submissionAgreementStore;

        public SubmissionAgreementsController(ISubmissionAgreementStore submissionAgreementStore)
        {
            this._submissionAgreementStore = submissionAgreementStore;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _submissionAgreementStore.GetAllIds();
        }
    }
}