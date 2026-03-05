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

            if (repo.AddReview(review) == 1)
            {
                return RedirectToAction("List");
            }
            return View();
        }
    }
}
