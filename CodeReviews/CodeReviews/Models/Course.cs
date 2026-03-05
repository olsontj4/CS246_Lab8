namespace CodeReviews.Models;

public class Course
{
    public int CourseId { get; set; }
    public string CoursePrefix { get; set; }  // Ex: CS
    public string CourseNumber { get; set; }  // Ex: 161
    public string CourseName { get; set; }   // Ex: Beginning Python
}