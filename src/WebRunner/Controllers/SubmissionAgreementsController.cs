using Microsoft.AspNetCore.Mvc;
using PoF.CaPM.SubmissionAgreements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace WebRunner.Controllers
{
    [Route("api/submissionagreements")]
    public class SubmissionAgreementsController : ControllerBase
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