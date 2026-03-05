using Xunit;
using CodeReviews.Controllers;
using CodeReviews.Models;

namespace CodeReviews.Tests;

/// <summary>
/// Unit tests for ReadinessCheckerController helper methods
/// </summary>
public class ReadinessCheckerControllerTests
{
    private readonly ReadinessCheckerController _controller;

    public ReadinessCheckerControllerTests()
    {
        _controller = new ReadinessCheckerController();
    }

    #region CalculateScore Tests

    [Fact]
    public void CalculateScore_AllCriticalItemsChecked_ReturnsExpectedScore()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = true,
            AppRuns = true,
            NoRuntimeErrors = true
        };

        // Act
        var score = _controller.CalculateScore(checklist);

        // Assert
        Assert.True(score >= 30); // At minimum 30 points for critical items
    }

    [Fact]
    public void CalculateScore_NoCriticalItemsChecked_ReturnsLowScore()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = false,
            AppRuns = false,
            NoRuntimeErrors = false
        };

        // Act
        var score = _controller.CalculateScore(checklist);

        // Assert
        Assert.True(score < 30); // Should be less than critical items total
    }

    [Fact]
    public void CalculateScore_AllItemsChecked_ReturnsMaxScore()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = true,
            AppRuns = true,
            NoRuntimeErrors = true,
            CodeIsClean = true,
            FollowsNamingConventions = true,
            HasErrorHandling = true,
            HasReadme = true,
            HasComments = true,
            HasTests = true,
            TestsPass = true,
            CommitsHaveMessages = true,
            NoSensitiveData = true
        };

        // Act
        var score = _controller.CalculateScore(checklist);
        var maxScore = _controller.GetMaxScore();

        // Assert
        Assert.Equal(maxScore, score);
    }

    #endregion

    #region DetermineStatus Tests

    [Fact]
    public void DetermineStatus_HighScore_ReturnsReady()
    {
        // Arrange
        int score = 85;
        int maxScore = 100;

        // Act
        var status = _controller.DetermineStatus(score, maxScore);

        // Assert
        Assert.Equal("Ready", status);
    }

    [Fact]
    public void DetermineStatus_MediumScore_ReturnsAlmostReady()
    {
        // Arrange
        int score = 65;
        int maxScore = 100;

        // Act
        var status = _controller.DetermineStatus(score, maxScore);

        // Assert
        Assert.Equal("Almost Ready", status);
    }

    [Fact]
    public void DetermineStatus_LowScore_ReturnsNotReady()
    {
        // Arrange
        int score = 40;
        int maxScore = 100;

        // Act
        var status = _controller.DetermineStatus(score, maxScore);

        // Assert
        Assert.Equal("Not Ready", status);
    }

    [Theory]
    [InlineData(80, 100, "Ready")]
    [InlineData(79, 100, "Almost Ready")]
    [InlineData(60, 100, "Almost Ready")]
    [InlineData(59, 100, "Not Ready")]
    public void DetermineStatus_VariousScores_ReturnsCorrectStatus(int score, int maxScore, string expectedStatus)
    {
        // Act
        var status = _controller.DetermineStatus(score, maxScore);

        // Assert
        Assert.Equal(expectedStatus, status);
    }

    #endregion

    #region GenerateMessage Tests

    [Fact]
    public void GenerateMessage_ReadyStatus_ReturnsPositiveMessage()
    {
        // Arrange
        string status = "Ready";

        // Act
        var message = _controller.GenerateMessage(status);

        // Assert
        Assert.NotEmpty(message);
        Assert.Contains("ready", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateMessage_NotReadyStatus_ReturnsConstructiveMessage()
    {
        // Arrange
        string status = "Not Ready";

        // Act
        var message = _controller.GenerateMessage(status);

        // Assert
        Assert.NotEmpty(message);
        Assert.Contains("work", message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region GenerateRecommendations Tests

    [Fact]
    public void GenerateRecommendations_ProjectDoesNotBuild_IncludesBuildRecommendation()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = false
        };

        // Act
        var recommendations = _controller.GenerateRecommendations(checklist);

        // Assert
        Assert.NotEmpty(recommendations);
        Assert.Contains(recommendations, r => r.Contains("build", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GenerateRecommendations_AllItemsChecked_ReturnsEmptyList()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = true,
            AppRuns = true,
            NoRuntimeErrors = true,
            CodeIsClean = true,
            FollowsNamingConventions = true,
            HasErrorHandling = true,
            HasReadme = true,
            HasComments = true,
            HasTests = true,
            TestsPass = true,
            CommitsHaveMessages = true,
            NoSensitiveData = true
        };

        // Act
        var recommendations = _controller.GenerateRecommendations(checklist);

        // Assert
        Assert.Empty(recommendations);
    }

    [Fact]
    public void GenerateRecommendations_MultipleMissingItems_ReturnsMultipleRecommendations()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = false,
            HasReadme = false,
            HasTests = false
        };

        // Act
        var recommendations = _controller.GenerateRecommendations(checklist);

        // Assert
        Assert.True(recommendations.Count > 1);
    }

    #endregion

    #region EvaluateReadiness Tests

    [Fact]
    public void EvaluateReadiness_ValidChecklist_ReturnsResultWithAllProperties()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = true,
            AppRuns = true,
            NoRuntimeErrors = true
        };

        // Act
        var result = _controller.EvaluateReadiness(checklist);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Score >= 0);
        Assert.True(result.MaxScore > 0);
        Assert.NotEmpty(result.Status);
        Assert.NotEmpty(result.Message);
        Assert.NotNull(result.Recommendations);
        Assert.NotNull(result.Checklist);
    }

    [Fact]
    public void EvaluateReadiness_PerfectChecklist_ReturnsReadyStatus()
    {
        // Arrange
        var checklist = new ReadinessChecklistViewModel
        {
            ProjectBuilds = true,
            AppRuns = true,
            NoRuntimeErrors = true,
            CodeIsClean = true,
            FollowsNamingConventions = true,
            HasErrorHandling = true,
            HasReadme = true,
            HasComments = true,
            HasTests = true,
            TestsPass = true,
            CommitsHaveMessages = true,
            NoSensitiveData = true
        };

        // Act
        var result = _controller.EvaluateReadiness(checklist);

        // Assert
        Assert.Equal("Ready", result.Status);
        Assert.Empty(result.Recommendations);
    }

    #endregion

    #region GetMaxScore Tests

    [Fact]
    public void GetMaxScore_ReturnsPositiveValue()
    {
        // Act
        var maxScore = _controller.GetMaxScore();

        // Assert
        Assert.True(maxScore > 0);
    }

    [Fact]
    public void GetMaxScore_ConsistentValue()
    {
        // Act
        var maxScore1 = _controller.GetMaxScore();
        var maxScore2 = _controller.GetMaxScore();

        // Assert
        Assert.Equal(maxScore1, maxScore2);
    }

    #endregion
}
