# Code Readiness Checker - Implementation Guide

## Overview
This feature allows students to check if their code is ready for peer review by completing a weighted checklist. The feature is built using MVC architecture with TDD (Test-Driven Development).

## What's Been Created

### Models
- **ReadinessChecklistViewModel**: Contains 12 boolean properties representing checklist items
- **ReadinessResultViewModel**: Contains the evaluation results (score, status, message, recommendations)

### Controller
- **ReadinessCheckerController**: Contains two action methods and five helper methods (stubs)

### Views
- **Index.cshtml**: Form with checklist grouped into 5 categories
- **Result.cshtml**: Displays the evaluation results with score, status, and recommendations

### Tests
- **ReadinessCheckerControllerTests**: 19 xUnit test cases covering all helper methods

## Current Status
? Branch created: `feature/readiness-checker`
? Structure complete with stub methods
? Views fully implemented
? Tests written (currently failing - this is expected!)
? Navigation link added

## What Students Need to Implement

### 1. CalculateScore Method
**Location**: `ReadinessCheckerController.cs` (line ~60)

**Requirements**:
- Assign weighted points to each checklist item
- Critical items (builds, runs, no errors): 10 points each
- Important items (clean code, naming, error handling): 5 points each
- Documentation items (README, comments): 4 points each
- Testing items (has tests, tests pass): 3 points each
- Repository items (commit messages, no sensitive data): 3 points each

**Total possible points should equal GetMaxScore() return value**

**Tests to pass**:
- `CalculateScore_AllCriticalItemsChecked_ReturnsExpectedScore`
- `CalculateScore_NoCriticalItemsChecked_ReturnsLowScore`
- `CalculateScore_AllItemsChecked_ReturnsMaxScore`

### 2. GetMaxScore Method
**Location**: `ReadinessCheckerController.cs` (line ~78)

**Requirements**:
- Calculate and return the maximum possible score
- Should match the sum of all weighted criteria
- Recommended: 100 points (adjust CalculateScore weights accordingly)

**Tests to pass**:
- `GetMaxScore_ReturnsPositiveValue`
- `GetMaxScore_ConsistentValue`

### 3. GenerateRecommendations Method
**Location**: `ReadinessCheckerController.cs` (line ~127)

**Requirements**:
- Check each unchecked item in the checklist
- Add specific, actionable recommendations for each missing item
- Return an empty list if all items are checked
- Prioritize critical items in recommendations

**Example recommendations**:
- "Fix all build errors before requesting a review."
- "Ensure your app runs without crashing on main pages."
- "Add a README with project description and setup instructions."

**Tests to pass**:
- `GenerateRecommendations_ProjectDoesNotBuild_IncludesBuildRecommendation`
- `GenerateRecommendations_AllItemsChecked_ReturnsEmptyList`
- `GenerateRecommendations_MultipleMissingItems_ReturnsMultipleRecommendations`

### 4. (Optional) Enhance DetermineStatus Method
**Location**: `ReadinessCheckerController.cs` (line ~92)

**Current Implementation**:
- Basic threshold logic (80%+ = Ready, 60-79% = Almost Ready, <60% = Not Ready)

**Enhancement Ideas**:
- Add additional status levels
- Require all critical items to be checked for "Ready" status regardless of score
- Add special handling for missing critical items

## Testing Workflow (TDD)

### Step 1: Run Tests (Red Phase)
```bash
dotnet test CodeReviews.Tests/CodeReviews.Tests.csproj
```
Current status: 16 passing, 3 failing ? (This is expected!)

### Step 2: Implement One Method (Green Phase)
Choose one method (recommended order):
1. GetMaxScore (easiest)
2. CalculateScore (core logic)
3. GenerateRecommendations (most complex)

### Step 3: Run Tests Again
After implementing each method, run tests to verify:
```bash
dotnet test CodeReviews.Tests/CodeReviews.Tests.csproj
```

### Step 4: Refactor (if needed)
Clean up your code while keeping tests passing.

### Step 5: Repeat for Remaining Methods

## Running the Application

1. Build the solution:
```bash
dotnet build
```

2. Run the application:
```bash
dotnet run --project CodeReviews/CodeReviews.csproj
```

3. Navigate to: `https://localhost:[port]/ReadinessChecker`

4. Test the feature:
   - Complete the checklist
   - Submit and view results
   - Verify recommendations appear for unchecked items

## Success Criteria

? All 19 tests pass
? Application builds without errors
? Checklist form displays correctly
? Result page shows accurate score and status
? Recommendations are helpful and specific
? Score calculation uses weighted criteria
? Edge cases handled (all checked, none checked, etc.)

## Additional Challenges (Optional)

1. **Add more checklist items** relevant to your course
2. **Create custom status levels** (e.g., "Needs Major Work", "Good to Go", "Excellent")
3. **Add scoring breakdown** by category in the result view
4. **Implement progress bar** showing percentage complete
5. **Add client-side validation** to ensure at least one item is checked
6. **Save results** to session/cookie to track progress

## File Locations

```
CodeReviews/
??? Controllers/
?   ??? ReadinessCheckerController.cs    ? Implement helper methods here
??? Models/
?   ??? ReadinessChecklistViewModel.cs
?   ??? ReadinessResultViewModel.cs
??? Views/
?   ??? ReadinessChecker/
?       ??? Index.cshtml
?       ??? Result.cshtml

CodeReviews.Tests/
??? ReadinessCheckerControllerTests.cs   ? Run these tests
```

## Questions to Consider

1. Why are the critical items weighted more heavily?
2. What happens if a student checks "Ready" but hasn't built their project?
3. How would you modify the scoring to make tests mandatory?
4. Could you make the weights configurable?

## Resources

- xUnit Documentation: https://xunit.net/
- ASP.NET Core MVC: https://docs.microsoft.com/aspnet/core/mvc/
- TDD Best Practices: https://martinfowler.com/bliki/TestDrivenDevelopment.html

---

**Happy Coding! Remember: Red ? Green ? Refactor** ??
