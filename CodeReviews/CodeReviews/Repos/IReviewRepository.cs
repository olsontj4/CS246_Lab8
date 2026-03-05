using CodeReviews.Models;

namespace CodeReviews.Repos
{
    public interface IReviewRepository
    {
        public List<Review> GetReviews();
        public Review GetReviewById(int id);
        public int AddReview(Review review);
    }
}
