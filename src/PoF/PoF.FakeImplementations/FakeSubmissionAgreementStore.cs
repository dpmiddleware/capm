using PoF.CaPM.SubmissionAgreements;
using PoF.Components.Archiver;
using PoF.Components.Collector;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.FakeImplementations
{
    public class FakeSubmissionAgreementStore : ISubmissionAgreementStore
    {
        private Dictionary<string, SubmissionAgreement> _submissionAgreements = new Dictionary<string, SubmissionAgreement>()
        {
            ["SubmissionAgreement1"] = new SubmissionAgreement()
            {
                SubmissionAgreementId = "SubmissionAgreement1",
                ProcessComponents = new SubmissionAgreement.ComponentExecutionPlan[]
                {
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.Collector.CollectorComponent.CollectorComponentIdentifier,
                        Order = 1
                    },
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.Archiver.ArchiverComponent.ArchiverComponentIdentifier,
                        Order = 2
                    }
                }
            },
            ["RandomErrorSubmissionAgreement"] = new SubmissionAgreement()
            {
                SubmissionAgreementId = "RandomErrorSubmissionAgreement",
                ProcessComponents = new SubmissionAgreement.ComponentExecutionPlan[]
                {
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.Collector.CollectorComponent.CollectorComponentIdentifier,
                        Order = 1
                    },
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.RandomError.RandomErrorComponent.RandomErrorComponentIdentifier,
                        Order = 2
                    }
                }
            }
        };

        public SubmissionAgreement Get(string id)
        {
            if (_submissionAgreements.ContainsKey(id))
            {
                return _submissionAgreements[id];
            }
            else
            {
                throw new KeyNotFoundException($"Could not find any submission agreement with the ID {id}.");
            }
        }
    }
}
