using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.SubmissionAgreements
{
    public class InMemorySubmissionAgreementStore : ISubmissionAgreementStore
    {
        private Dictionary<string, SubmissionAgreement> _submissionAgreements = new Dictionary<string, SubmissionAgreement>();

        public SubmissionAgreement Get(string id)
        {
            if (_submissionAgreements.ContainsKey(id))
            {
                return _submissionAgreements[id];
            }
            else
            {
                throw new KeyNotFoundException($"Could not find submission agreement with ID '{id}'");
            }
        }

        public void Add(SubmissionAgreement submissionAgreement)
        {
            if (_submissionAgreements.ContainsKey(submissionAgreement.SubmissionAgreementId))
            {
                throw new ArgumentException($"There is already a registered submission agreement with the id '{submissionAgreement.SubmissionAgreementId}'");
            }
            _submissionAgreements.Add(submissionAgreement.SubmissionAgreementId, submissionAgreement);
        }
    }
}
