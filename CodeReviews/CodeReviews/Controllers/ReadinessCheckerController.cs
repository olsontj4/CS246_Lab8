using Microsoft.AspNetCore.Mvc;
using CodeReviews.Models;

namespace CodeReviews.Controllers;

/// <summary>
/// Controller for the Code Readiness Checker feature
/// </summary>
public class ReadinessCheckerController : Controller
{
    // GET: ReadinessChecker
    public IActionResult Index()
    {
        return View(new ReadinessChecklistViewModel());
    }

    // POST: ReadinessChecker
    [HttpPost]
    public IActionResult Index(ReadinessChecklistViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = EvaluateReadiness(model);
        return View("Result", result);
    }

    /// <summary>
    /// Evaluates the readiness checklist and returns a result with score and recommendations
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>ReadinessResultViewModel with score and status</returns>
    public ReadinessResultViewModel EvaluateReadiness(ReadinessChecklistViewModel checklist)
    {
        var score = CalculateScore(checklist);
        var maxScore = GetMaxScore();
        var status = DetermineStatus(score, maxScore);
        var message = GenerateMessage(status);
        var recommendations = GenerateRecommendations(checklist);

        return new ReadinessResultViewModel
        {
            Score = score,
            MaxScore = maxScore,
            Status = status,
            Message = message,
            Recommendations = recommendations,
            Checklist = checklist
        };
    }

    /// <summary>
    /// Calculates the total score based on weighted criteria
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>Total score</returns>
    public int CalculateScore(ReadinessChecklistViewModel checklist)
    {
        int score = 0;

        // Critical criteria (10 points each = 30 points total)
        if (checklist.ProjectBuilds) score += 10;
        if (checklist.AppRuns) score += 10;
        if (checklist.NoRuntimeErrors) score += 10;

        // Important criteria (8 points each = 24 points total)
        if (checklist.CodeIsClean) score += 8;
        if (checklist.FollowsNamingConventions) score += 8;
        if (checklist.HasErrorHandling) score += 8;

        // Documentation (7 points each = 14 points total)
        if (checklist.HasReadme) score += 7;
        if (checklist.HasComments) score += 7;

        // Testing (8 points each = 16 points total)
        if (checklist.HasTests) score += 8;
        if (checklist.TestsPass) score += 8;

        // Repository (8 points each = 16 points total)
        if (checklist.CommitsHaveMessages) score += 8;
        if (checklist.NoSensitiveData) score += 8;

        return score;
    }

    /// <summary>
    /// Gets the maximum possible score
    /// </summary>
    /// <returns>Maximum score</returns>
    public int GetMaxScore()
    {
        // 30 (critical) + 24 (important) + 14 (docs) + 16 (testing) + 16 (repo) = 100
        return 100;
    }

    /// <summary>
    /// Determines readiness status based on score
    /// </summary>
    /// <param name="score">Current score</param>
    /// <param name="maxScore">Maximum possible score</param>
    /// <returns>Status: "Ready", "Almost Ready", or "Not Ready"</returns>
    public string DetermineStatus(int score, int maxScore)
    {
        double percentage = (double)score / maxScore * 100;

        if (percentage >= 80) return "Ready";
        if (percentage >= 60) return "Almost Ready";
        return "Not Ready";
    }

    /// <summary>
    /// Generates a message based on readiness status
    /// </summary>
    /// <param name="status">The readiness status</param>
    /// <returns>User-friendly message</returns>
    public string GenerateMessage(string status)
    {
        return status switch
        {
            "Ready" => "Great! Your code is ready for review.",
            "Almost Ready" => "You're close! Address a few more items before requesting a review.",
            "Not Ready" => "Your code needs more work before it's ready for review.",
            _ => "Unable to determine readiness."
        };
    }

    /// <summary>
    /// Generates a list of recommendations based on unchecked items
    /// </summary>
    /// <param name="checklist">The completed checklist</param>
    /// <returns>List of recommendations</returns>
    public List<string> GenerateRecommendations(ReadinessChecklistViewModel checklist)
    {
        var recommendations = new List<string>();

        // Critical items
        if (!checklist.ProjectBuilds)
        {
            recommendations.Add("Fix all build errors before requesting a review.");
        }
        if (!checklist.AppRuns)
        {
            recommendations.Add("Ensure your application runs without crashing on startup.");
        }
        if (!checklist.NoRuntimeErrors)
        {
            recommendations.Add("Test all main pages and features to ensure they work without runtime errors.");
        }

        // Important items
        if (!checklist.CodeIsClean)
        {
            recommendations.Add("Remove debug statements, commented-out code, and unused variables.");
        }
        if (!checklist.FollowsNamingConventions)
        {
            recommendations.Add("Follow C# naming conventions: PascalCase for classes/methods, camelCase for local variables.");
        }
        if (!checklist.HasErrorHandling)
        {
            recommendations.Add("Add try-catch blocks and validation for user input and external operations.");
        }

        // Documentation
        if (!checklist.HasReadme)
        {
            recommendations.Add("Create a README.md with project description, features, and setup instructions.");
        }
        if (!checklist.HasComments)
        {
            recommendations.Add("Add comments to explain complex logic and algorithm choices.");
        }

        // Testing
        if (!checklist.HasTests)
        {
            recommendations.Add("Consider adding unit tests for business logic and controller methods.");
        }
        if (checklist.HasTests && !checklist.TestsPass)
        {
            recommendations.Add("Fix failing tests before requesting a review.");
        }

        // Repository
        if (!checklist.CommitsHaveMessages)
        {
            recommendations.Add("Write clear, descriptive commit messages that explain what and why.");
        }
        if (!checklist.NoSensitiveData)
        {
            recommendations.Add("Remove passwords, API keys, and connection strings from your repository.");
        }

        return recommendations;
    }
}
