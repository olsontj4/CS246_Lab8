namespace CodeReviews.Models
{
    public class Assignment
    {
        public int AssignmentId { get; set; }
        public required Section ClassSection { get; set; }
        public required string AssignmentName { get; set; }  // Ex: Lab01-Variables
        public DateOnly DraftDueDate { get; set; }
        public DateOnly ReviewDueDate { get; set; }
        public DateOnly FinalDueDate { get; set; }
        public List<string>? Versions { get; set; }  // Ex: A, B, C
    }
}
