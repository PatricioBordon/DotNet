using BookManagementApi.DTOs;
using BookManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AuthorResponse>> CreateAuthor([FromBody] CreateAuthorRequest request)
        {
            var author = await _authorService.CreateAuthorAsync(request);
            return CreatedAtAction(nameof(GetAuthors), new { id = author.Id }, author);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PagedResult<AuthorResponse>>> GetAuthors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _authorService.GetAuthorsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<ActionResult<AuthorResponse>> UpdateAuthor(Guid id, [FromBody] UpdateAuthorRequest request)
        {
            var author = await _authorService.UpdateAuthorAsync(id, request);
            if (author == null)
                return NotFound(new { message = $"Author with ID {id} not found" });

            return Ok(author);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var result = await _authorService.DeleteAuthorAsync(id);
            if (!result)
                return NotFound(new { message = $"Author with ID {id} not found" });

            return NoContent();
        }
    }
}