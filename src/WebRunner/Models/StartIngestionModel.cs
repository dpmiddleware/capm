using System.ComponentModel.DataAnnotations;

namespace WebRunner.Models
{
    public class StartIngestionModel
    {
        [Required]
        public string SubmissionAgreementId { get; set; }
        public string IngestParameters { get; set; }
    }
}