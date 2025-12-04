using BookManagementApi.Data;
using BookManagementApi.DTOs;
using BookManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace BookManagementApi.Services
{
    public interface IAuthorService
    {
        Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request);
        Task<PagedResult<AuthorResponse>> GetAuthorsAsync(int pageNumber, int pageSize);
        Task<AuthorResponse?> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request);
        Task<bool> DeleteAuthorAsync(Guid id);
    }

    public class AuthorService : IAuthorService
    {
        private readonly ApplicationDbContext _context;

        public AuthorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthorResponse> CreateAuthorAsync(CreateAuthorRequest request)
        {
            var normalizedName = NormalizeString(request.Name);

            var author = new Author
            {
                Id = Guid.NewGuid(),
                Name = normalizedName
            };

            _context.Authors.Add(author);
            await _context.SaveChangesAsync();

            return MapToAuthorResponse(author);
        }

        public async Task<PagedResult<AuthorResponse>> GetAuthorsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Authors.AsQueryable();
            
            var totalCount = await query.CountAsync();
            var authors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AuthorResponse>
            {
                Items = authors.Select(MapToAuthorResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<AuthorResponse?> UpdateAuthorAsync(Guid id, UpdateAuthorRequest request)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                author.Name = NormalizeString(request.Name);
            }

            await _context.SaveChangesAsync();
            return MapToAuthorResponse(author);
        }

        public async Task<bool> DeleteAuthorAsync(Guid id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return false;

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }

        private AuthorResponse MapToAuthorResponse(Author author)
        {
            return new AuthorResponse
            {
                Id = author.Id,
                Name = author.Name
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