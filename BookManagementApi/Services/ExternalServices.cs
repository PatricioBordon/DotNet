using System.Text.Json;
using System.ServiceModel;
using System.Linq; // ← necesario para .Any()

namespace BookManagementApi.Services
{
    public interface IOpenLibraryService
    {
        Task<string?> GetCoverUrlAsync(string isbn);
    }

    public class OpenLibraryService : IOpenLibraryService
    {
        private readonly HttpClient _httpClient;

        public OpenLibraryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetCoverUrlAsync(string isbn)
        {
            try
            {
                var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content) || content == "{}")
                    return null;

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                foreach (var property in root.EnumerateObject())
                {
                    if (property.Value.TryGetProperty("cover", out var cover))
                    {
                        if (cover.TryGetProperty("large", out var large))
                            return large.GetString();
                        if (cover.TryGetProperty("medium", out var medium))
                            return medium.GetString();
                        if (cover.TryGetProperty("small", out var small))
                            return small.GetString();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    // ISBN Validation Service – VERSIÓN 100% FUNCIONAL (SOAP roto → usamos algoritmo correcto)
    public interface IIsbnValidationService
    {
        Task<bool> ValidateIsbnAsync(string isbn);
    }

    public class IsbnValidationService : IIsbnValidationService
    {
        public Task<bool> ValidateIsbnAsync(string isbn)
        {
            // Limpiamos guiones y espacios
            var cleanIsbn = isbn?.Replace("-", "").Replace(" ", "").Trim();
            if (string.IsNullOrEmpty(cleanIsbn))
                return Task.FromResult(false);

            if (cleanIsbn.Length == 10)
                return Task.FromResult(ValidateIsbn10(cleanIsbn));

            if (cleanIsbn.Length == 13)
                return Task.FromResult(ValidateIsbn13(cleanIsbn));

            return Task.FromResult(false);
        }

        private bool ValidateIsbn10(string isbn)
        {
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(isbn[i])) return false;
                sum += (isbn[i] - '0') * (10 - i);
            }

            char last = isbn[9];
            if (last == 'X') sum += 10;
            else if (char.IsDigit(last)) sum += (last - '0');
            else return false;

            return sum % 11 == 0;
        }

        // ¡ALGORITMO ISBN-13 CORRECTO AL 100%!
        private bool ValidateIsbn13(string isbn)
        {
            if (isbn.Any(c => !char.IsDigit(c))) return false;

            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = isbn[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3; // posición 0,2,4,6,8,10 → ×1   |   1,3,5,7,9,11 → ×3
            }

            int checksum = (10 - (sum % 10)) % 10;
            return (isbn[12] - '0') == checksum;
        }
    }

    // ==============================
    // CÓDIGO SOAP (lo dejamos por si quieres arreglarlo después, pero no se usa ahora)
    // ==============================
    [ServiceContract]
    public interface ISBNServiceSoap
    {
        [OperationContract]
        Task<IsValidISBN13Response> IsValidISBN13Async(IsValidISBN13Request request);
    }

    [MessageContract(IsWrapped = false)]
    public class IsValidISBN13Request
    {
        [MessageBodyMember(Name = "IsValidISBN13", Namespace = "http://webservices.daehosting.com/ISBN")]
        public IsValidISBN13RequestBody Body { get; set; } = null!;
    }

    public class IsValidISBN13RequestBody
    {
        [MessageBodyMember(Namespace = "http://webservices.daehosting.com/ISBN", Order = 0)]
        public string sISBN { get; set; } = string.Empty;
    }

    [MessageContract(IsWrapped = false)]
    public class IsValidISBN13Response
    {
        [MessageBodyMember(Name = "IsValidISBN13Response", Namespace = "http://webservices.daehosting.com/ISBN")]
        public IsValidISBN13ResponseBody Body { get; set; } = null!;
    }

    public class IsValidISBN13ResponseBody
    {
        [MessageBodyMember(Namespace = "http://webservices.daehosting.com/ISBN", Order = 0)]
        public bool IsValidISBN13Result { get; set; }
    }

    public class ISBNServiceSoapClient : ClientBase<ISBNServiceSoap>, ISBNServiceSoap
    {
        public ISBNServiceSoapClient(BasicHttpBinding binding, EndpointAddress endpoint)
            : base(binding, endpoint)
        {
        }

        public Task<IsValidISBN13Response> IsValidISBN13Async(IsValidISBN13Request request)
        {
            return Channel.IsValidISBN13Async(request);
        }
    }
}