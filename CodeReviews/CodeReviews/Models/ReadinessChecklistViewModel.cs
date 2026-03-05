namespace CodeReviews.Models;

/// <summary>
/// ViewModel for the Code Readiness Checker form
/// </summary>
public class ReadinessChecklistViewModel
{
    // Build & Run
    public bool ProjectBuilds { get; set; }
    public bool AppRuns { get; set; }
    public bool NoRuntimeErrors { get; set; }

    // Code Quality
    public bool CodeIsClean { get; set; }
    public bool FollowsNamingConventions { get; set; }
    public bool HasErrorHandling { get; set; }

    // Documentation
    public bool HasReadme { get; set; }
    public bool HasComments { get; set; }

    // Testing
    public bool HasTests { get; set; }
    public bool TestsPass { get; set; }

    // Repository
    public bool CommitsHaveMessages { get; set; }
    public bool NoSensitiveData { get; set; }
}
