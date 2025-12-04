using BookManagementApi.Data;
using BookManagementApi.DTOs;
using BookManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace BookManagementApi.Services
{
    public interface IBookService
    {
        Task<BookResponse> CreateBookAsync(CreateBookRequest request);
        Task<PagedResult<BookResponse>> GetBooksAsync(int pageNumber, int pageSize, string? title, string? authorName);
        Task<BookResponse?> GetBookByIdAsync(Guid id);
        Task<BookResponse?> UpdateBookAsync(Guid id, UpdateBookRequest request);
        Task<bool> DeleteBookAsync(Guid id);
        Task<MassiveUploadResult> CreateBooksMassiveAsync(List<CsvBookRecord> records);
    }

    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIsbnValidationService _isbnValidationService;
        private readonly IOpenLibraryService _openLibraryService;

        public BookService(
            ApplicationDbContext context,
            IIsbnValidationService isbnValidationService,
            IOpenLibraryService openLibraryService)
        {
            _context = context;
            _isbnValidationService = isbnValidationService;
            _openLibraryService = openLibraryService;
        }

        public async Task<BookResponse> CreateBookAsync(CreateBookRequest request)
        {
            var isValidIsbn = await _isbnValidationService.ValidateIsbnAsync(request.Isbn);
            if (!isValidIsbn)
            {
                throw new InvalidOperationException($"Invalid ISBN: {request.Isbn}");
            }

            var coverUrl = await _openLibraryService.GetCoverUrlAsync(request.Isbn);
            var normalizedTitle = NormalizeString(request.Title);
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Isbn = request.Isbn,
                Title = normalizedTitle,
                CoverUrl = coverUrl,
                PublicationYear = request.PublicationYear,
                AuthorId = request.AuthorId
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return await MapToBookResponse(book);
        }

        public async Task<PagedResult<BookResponse>> GetBooksAsync(
            int pageNumber, int pageSize, string? title, string? authorName)
        {
            var query = _context.Books.Include(b => b.Author).AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                var normalizedTitle = NormalizeString(title);
                query = query.Where(b => b.Title.Contains(normalizedTitle));
            }

            if (!string.IsNullOrWhiteSpace(authorName))
            {
                var normalizedAuthorName = NormalizeString(authorName);
                query = query.Where(b => b.Author.Name.Contains(normalizedAuthorName));
            }

            var totalCount = await query.CountAsync();
            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var bookResponses = new List<BookResponse>();
            foreach (var book in books)
            {
                bookResponses.Add(await MapToBookResponse(book));
            }

            return new PagedResult<BookResponse>
            {
                Items = bookResponses,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BookResponse?> GetBookByIdAsync(Guid id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            return book == null ? null : await MapToBookResponse(book);
        }

        public async Task<BookResponse?> UpdateBookAsync(Guid id, UpdateBookRequest request)
        {
            var book = await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Isbn))
            {
                var isValidIsbn = await _isbnValidationService.ValidateIsbnAsync(request.Isbn);
                if (!isValidIsbn)
                {
                    throw new InvalidOperationException($"Invalid ISBN: {request.Isbn}");
                }
                book.Isbn = request.Isbn;
                book.CoverUrl = await _openLibraryService.GetCoverUrlAsync(request.Isbn);
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                book.Title = NormalizeString(request.Title);
            }

            if (request.PublicationYear.HasValue)
            {
                book.PublicationYear = request.PublicationYear.Value;
            }

            if (request.AuthorId.HasValue)
            {
                book.AuthorId = request.AuthorId.Value;
            }

            await _context.SaveChangesAsync();
            return await MapToBookResponse(book);
        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MassiveUploadResult> CreateBooksMassiveAsync(List<CsvBookRecord> records)
        {
            var result = new MassiveUploadResult { TotalRecords = records.Count };

            foreach (var record in records)
            {
                try
                {
                    // Find or create author
                    var normalizedAuthorName = NormalizeString(record.AuthorName);
                    var author = await _context.Authors
                        .FirstOrDefaultAsync(a => a.Name == normalizedAuthorName);

                    if (author == null)
                    {
                        author = new Author
                        {
                            Id = Guid.NewGuid(),
                            Name = normalizedAuthorName
                        };
                        _context.Authors.Add(author);
                        await _context.SaveChangesAsync();
                    }

                    // Create book
                    var createRequest = new CreateBookRequest
                    {
                        Isbn = record.Isbn,
                        Title = record.Title,
                        PublicationYear = record.PublicationYear,
                        AuthorId = author.Id
                    };

                    await CreateBookAsync(createRequest);
                    result.SuccessfulRecords++;
                }
                catch (Exception ex)
                {
                    result.FailedRecords++;
                    result.Errors.Add($"ISBN {record.Isbn}: {ex.Message}");
                }
            }

            return result;
        }

        private async Task<BookResponse> MapToBookResponse(Book book)
        {
            if (book.Author == null)
            {
                var author = await _context.Authors.FindAsync(book.AuthorId);
                book.Author = author!;
            }

            return new BookResponse
            {
                Id = book.Id,
                Isbn = book.Isbn,
                Title = book.Title,
                CoverUrl = book.CoverUrl,
                PublicationYear = book.PublicationYear,
                AuthorId = book.AuthorId,
                AuthorName = book.Author.Name
            };
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var result = input.ToUpper();
            result = Regex.Replace(result, @"\d", "");
            result = RemoveDiacritics(result);
            result = Regex.Replace(result, @"\s+", " ");

            return result.Trim();
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}