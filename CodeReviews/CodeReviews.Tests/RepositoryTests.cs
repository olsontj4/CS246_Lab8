using CodeReviews.Controllers;
using CodeReviews.Models;
using CodeReviews.Repos;
using Microsoft.AspNetCore.Mvc;

namespace CodeReviews.Tests
{
    public class RepositoryTests
    {
        IReviewRepository _repo = new FakeReviewRepository();
        ReviewController _controller;
        Review _review = new Review();
        public RepositoryTests() {
            _controller = new ReviewController(_repo);
            AppUser user = new AppUser();
            _review.Reviewer = user;
            _review.ReviewDate = DateTime.Now;
            _review.ReviewUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
            _review.ReviewId = 0;
            _repo.AddReview(_review);
        }
        [Fact]
        public void GetReviewsTest()
        {
            Assert.Equal(1, _repo.GetReviews().Count);//One initial review.
        }
        [Fact]
        public void AddReviewSuccess()
        {
            Review review = new();
            review.Reviewer = new AppUser();
            review.ReviewDate = DateTime.Now;
            review.ReviewUrl = "https://www.google.com";
            review.ReviewId = 1;
            int status = _repo.AddReview(review);
            Assert.Equal(1, status);//Status code 1.
            Assert.Equal(2, _repo.GetReviews().Count);//One initial review plus one new review.
        }
        [Fact]
        public void GetReviewByIdSuccess()
        {
            Review review = _repo.GetReviewById(0);
            Assert.NotNull( review);//One initial review.
            Assert.NotNull(review.Reviewer);
            Assert.NotNull(review.ReviewDate);
            Assert.Equal("https://www.youtube.com/watch?v=dQw4w9WgXcQ", review.ReviewUrl);
        }
        [Fact]
        public void GetReviewByIdFail()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _repo.GetReviewById(1));//No second review.
        }
        [Fact]
        public void Review_PostTest_Success()
        {
            // arrange
            Review review = new();
            review.Reviewer = new AppUser();
            review.ReviewDate = DateTime.Now;
            review.ReviewUrl = "https://www.google.com";
            review.ReviewId = 1;

            // act
            var result = _controller.Review(review);

            // assert: check to see if I got a RedirectToActionResult
            Assert.True(result.GetType() == typeof(RedirectToActionResult));
        }
        [Fact]
        public void Review_PostTest_Failure()
        {
            // arrange
            Review review = new();

            // act
            var result = _controller.Review(review);

            // assert: check to see if I got a RedirectToActionResult
            Assert.True(result.GetType() == typeof(ViewResult));
        }
    }
}
