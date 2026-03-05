namespace CodeReviews.Models;

public class Section  // Course Section
{
    public int SectionId { get; set; }
    public Course Course { get; set; }
    public int SectionNumber { get; set; }  // CRN
    public string Day { get; set; } // Ex: MW or TuTh
    public string StartTime { get; set; } // Ex: 10:00 AM
    public string Modality { get; set; } // Ex: Hybrid or Online
    public string Term { get; set; } // Ex: Fall
    public int Year { get; set; } // Ex: 2026
    public AppUser Instructor { get; set; }
}