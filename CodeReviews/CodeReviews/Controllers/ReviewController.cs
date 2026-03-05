using CodeReviews.Models;
using CodeReviews.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CodeReviews.Controllers
{
    public class ReviewController : Controller
    {
        // private instance variable
        IReviewRepository repo;

        // constructor
        public ReviewController(IReviewRepository r)
        {
            repo = r;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var reviews = repo.GetReviews();
            return View(reviews);
        }

        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Review(Review review)
        {
            review.ReviewDate = DateTime.Now;

            // TODO: Remove this dummy submission after Submission entry code is done
            review.Submission = new Submission
            {
                CodeUrl = review.Submission?.CodeUrl ?? "",
                Version = "A",
                SubmissionDate = DateTime.Now,
                Student = new AppUser { Name = "Dummy Student" },
                Assignment = new Assignment
                {
                    AssignmentName = "Dummy Assignment",
                    DraftDueDate = DateOnly.FromDateTime(DateTime.Now),
                    ReviewDueDate = DateOnly.FromDateTime(DateTime.Now),
                    FinalDueDate = DateOnly.FromDateTime(DateTime.Now),
                    ClassSection = new Section
                    {
                        SectionNumber = 0,
                        Day = "TBD",
                        StartTime = "TBD",
                        Modality = "TBD",
                        Term = "TBD",
                        Year = DateTime.Now.Year,
                        Course = new Course
                        {
                            CoursePrefix = "CS",
                            CourseNumber = "000",
                            CourseName = "Dummy Course"
                        },
                        Instructor = new AppUser { Name = "Dummy Instructor" }
                    }
                }
            };

            if (repo.AddReview(review) == 1)
            {
                return RedirectToAction("List");
            }
            return View();
        }
    }
}
