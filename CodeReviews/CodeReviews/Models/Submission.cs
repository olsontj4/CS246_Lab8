namespace CodeReviews.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public Assignment Assignment { get; set; }
        public string Version { get; set; } // Ex: A, B, C
        public AppUser Student { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string CodeUrl { get; set; } // URL to the student's code submission
    }
}
