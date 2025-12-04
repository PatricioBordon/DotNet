using System.ComponentModel.DataAnnotations;

namespace BookManagementApi.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string Isbn { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        
        public string? CoverUrl { get; set; }
        
        [Range(1000, 9999)]
        public int PublicationYear { get; set; }
        
        public Guid AuthorId { get; set; }
        public Author Author { get; set; } = null!;
    }

    public class Author
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }

    public class User
    {
        public Guid Id { get; set; }
        
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string HashedPassword { get; set; } = string.Empty;
    }
}

namespace BookManagementApi.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateBookRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string Isbn { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;
        
        [Range(1000, 9999)]
        public int PublicationYear { get; set; }
        
        [Required]
        public Guid AuthorId { get; set; }
    }

    public class UpdateBookRequest
    {
        [StringLength(20, MinimumLength = 10)]
        public string? Isbn { get; set; }
        
        [StringLength(200, MinimumLength = 1)]
        public string? Title { get; set; }
        
        [Range(1000, 9999)]
        public int? PublicationYear { get; set; }
        
        public Guid? AuthorId { get; set; }
    }

    public class BookResponse
    {
        public Guid Id { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public int PublicationYear { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class CreateAuthorRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateAuthorRequest
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }
    }

    public class AuthorResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class IsbnValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CsvBookRecord
    {
        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class MassiveUploadResult
    {
        public int TotalRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}