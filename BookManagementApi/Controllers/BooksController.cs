using BookManagementApi.DTOs;
using BookManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IIsbnValidationService _isbnValidationService;

        public BooksController(IBookService bookService, IIsbnValidationService isbnValidationService)
        {
            _bookService = bookService;
            _isbnValidationService = isbnValidationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BookResponse>> CreateBook([FromBody] CreateBookRequest request)
        {
            var book = await _bookService.CreateBookAsync(request);
            return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<PagedResult<BookResponse>>> GetBooks(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? title = null,
            [FromQuery] string? authorName = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookService.GetBooksAsync(pageNumber, pageSize, title, authorName);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<BookResponse>> GetBookById(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return Ok(book);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<ActionResult<BookResponse>> UpdateBook(Guid id, [FromBody] UpdateBookRequest request)
        {
            var book = await _bookService.UpdateBookAsync(id, request);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return Ok(book);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return NoContent();
        }

        [HttpGet("validation/{isbn}")]
        [Authorize]
        public async Task<ActionResult<IsbnValidationResponse>> ValidateIsbn(string isbn)
        {
            var isValid = await _isbnValidationService.ValidateIsbnAsync(isbn);
            return Ok(new IsbnValidationResponse
            {
                IsValid = isValid,
                Message = isValid ? "ISBN is valid" : "ISBN is invalid"
            });
        }

        [HttpPost("masive")]
        [Authorize]
        public async Task<ActionResult<MassiveUploadResult>> MassiveUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only CSV files are allowed" });

            var records = new List<CsvBookRecord>();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string? line;
                bool isFirstLine = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    var values = line.Split(',');
                    if (values.Length >= 4)
                    {
                        if (int.TryParse(values[2].Trim(), out int year))
                        {
                            records.Add(new CsvBookRecord
                            {
                                Isbn = values[0].Trim(),
                                Title = values[1].Trim(),
                                PublicationYear = year,
                                AuthorName = values[3].Trim()
                            });
                        }
                    }
                }
            }

            if (records.Count == 0)
                return BadRequest(new { message = "No valid records found in CSV" });

            var result = await _bookService.CreateBooksMassiveAsync(records);
            return Ok(result);
        }
    }
}