﻿using PoF.CaPM.SubmissionAgreements;
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
                        ExecutionTimeoutInSeconds = 180,
                        Order = 1
                    },
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.Archiver.ArchiverComponent.ArchiverComponentIdentifier,
                        ExecutionTimeoutInSeconds = 180,
                        Order = 2,
                        ComponentSettings = "http://localhost:59669/api/aips"
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
                        ExecutionTimeoutInSeconds = 180,
                        Order = 1
                    },
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.RandomError.RandomErrorComponent.RandomErrorComponentIdentifier,
                        ExecutionTimeoutInSeconds = 180,
                        Order = 2
                    },
                    new SubmissionAgreement.ComponentExecutionPlan()
                    {
                        ComponentCode = PoF.Components.Archiver.ArchiverComponent.ArchiverComponentIdentifier,
                        ExecutionTimeoutInSeconds = 10,
                        Order = 3,
                        ComponentSettings = "http://localhost:59669/api/aips"
                    }
                }
            },
            ["1% Failing and 1% Failing on Compensation"] = new SubmissionAgreement()
            {
                SubmissionAgreementId = "1% Failing and 1% Failing on Compensation",
                ProcessComponents = new[]
                {
                    new SubmissionAgreement.ComponentExecutionPlan
                    {
                        ComponentCode = "Component.Collector",
                        ComponentSettings = "{\"CompensationFailureRisk\": 0.01}",
                        Order = 1
                    },
                    new SubmissionAgreement.ComponentExecutionPlan
                    {
                        ComponentCode= "Component.RandomError",
                        ComponentSettings= "{\"FailureRisk\": 0.01}",
                        Order= 2
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

        public string[] GetAllIds()
        {
            return _submissionAgreements.Keys.ToArray();
        }

        public void Add(string id, SubmissionAgreement submissionAgreement)
        {
            _submissionAgreements.Add(id, submissionAgreement);
        }
    }
}
