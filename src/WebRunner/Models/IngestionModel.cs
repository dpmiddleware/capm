using System.ComponentModel.DataAnnotations;

namespace WebRunner.Models
{
    public class IngestionModel
    {
        [Required]
        public string SubmissionAgreementId { get; set; }
        public string IngestParameters { get; set; }
    }
}