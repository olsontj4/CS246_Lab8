using CodeReviews.Data;
using CodeReviews.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeReviews.Repos
{
    public class ReviewRepository : IReviewRepository
    {
        private AppDbContext context;

        public ReviewRepository(AppDbContext appDbContext)
        {
            context = appDbContext;
        }
        public int AddReview(Review review)
        {
            context.Reviews.Add(review);
            return context.SaveChanges();
        }

        public Review GetReviewById(int id)
        {
            var review = context.Reviews
                .Include(r => r.Reviewer) // returns Reivew.AppUser object
                .Include(r=> r.Submission)
                .Where(r => r.ReviewId == id)
                .SingleOrDefault();
            return review;
        }

        public List<Review> GetReviews()
        {
            var reveiws = context.Reviews
              .Include(r => r.Reviewer) // returns Reivew.AppUser object
              .Include(r => r.Submission)
              .ToList<Review>();
            return reveiws;
        }
    }
}
