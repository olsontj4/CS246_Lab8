using CodeReviews.Models;
using CodeReviews.Repos;

namespace CodeReviews.Tests
{
    public class FakeReviewRepository : IReviewRepository
    {
        private List<Review> reviews = new List<Review>();  // Use a list as a data store

        public int AddReview(Review review)
        {
            int status = 0;
            if (review != null && review.Reviewer != null)
            {
                review.ReviewId = reviews.Count + 1;
                reviews.Add(review);
                status = 1;
            }
            return status;
        }

        public Review GetReviewById(int id)
        {
            Review review = reviews[id];
            return review;
        }

        public List<Review> GetReviews()
        {
            return reviews;
        }
    }
}