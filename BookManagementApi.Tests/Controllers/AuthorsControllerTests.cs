using BookManagementApi.Controllers;
using BookManagementApi.DTOs;
using BookManagementApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BookManagementApi.Tests.Controllers
{
    public class AuthorsControllerTests
    {
        [Fact]
        public async Task CreateAuthor_ValidRequest_ReturnsCreatedResult()
        {
            var mockService = new Mock<IAuthorService>();
            var request = new CreateAuthorRequest { Name = "Gabriel García Márquez" };
            var expectedResponse = new AuthorResponse 
            { 
                Id = Guid.NewGuid(), 
                Name = "GABRIEL GARCÍA MÁRQUEZ" 
            };

            mockService.Setup(x => x.CreateAuthorAsync(request))
                       .ReturnsAsync(expectedResponse);

            var controller = new AuthorsController(mockService.Object);
            var result = await controller.CreateAuthor(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetAuthors", createdResult.ActionName);
            
            var returnedAuthor = Assert.IsType<AuthorResponse>(createdResult.Value);
            Assert.Equal(expectedResponse.Id, returnedAuthor.Id);
            Assert.Contains("GARCÍA MÁRQUEZ", returnedAuthor.Name);
        }
    }
}