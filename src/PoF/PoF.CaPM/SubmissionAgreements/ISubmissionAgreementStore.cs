using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.SubmissionAgreements
{
    public interface ISubmissionAgreementStore
    {
        SubmissionAgreement Get(string id);
    }
}
