using BookManagementApi.Controllers;
using BookManagementApi.DTOs;
using BookManagementApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BookManagementApi.Tests.Controllers
{
    public class BooksControllerTests
    {
        [Fact]
        public async Task CreateBook_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            var mockBookService = new Mock<IBookService>();
            var mockIsbnService = new Mock<IIsbnValidationService>();

            var request = new CreateBookRequest
            {
                Isbn = "9780307474728",
                Title = "Cien años de soledad",
                PublicationYear = 1967,
                AuthorId = Guid.NewGuid()
            };

            var expectedBook = new BookResponse
            {
                Id = Guid.NewGuid(),
                Title = "CIEN AÑOS DE SOLEDAD",
                Isbn = request.Isbn,
                CoverUrl = "https://covers.openlibrary.org/b/id/123-L.jpg",
                PublicationYear = request.PublicationYear,
                AuthorId = request.AuthorId,
                AuthorName = "Gabriel García Márquez"
            };

            mockBookService
                .Setup(x => x.CreateBookAsync(It.IsAny<CreateBookRequest>()))
                .ReturnsAsync(expectedBook);

            var controller = new BooksController(mockBookService.Object, mockIsbnService.Object);

            // Act
            var result = await controller.CreateBook(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetBookById", createdResult.ActionName);
            Assert.Equal(expectedBook.Id, ((BookResponse)createdResult.Value!).Id);

            var returnedBook = createdResult.Value as BookResponse;
            Assert.NotNull(returnedBook);
            Assert.Contains("CIEN AÑOS", returnedBook.Title);
        }
    }
}