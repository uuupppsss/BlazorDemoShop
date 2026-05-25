using LibDemoShop;
using System.Net;
using System.Net.Http.Headers;

namespace BlazorDemoShop.Services
{
    public class ApiTagsService
    {
        private readonly HttpClient _httpClient;

        public ApiTagsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ================= TYPES =================

        public async Task<List<ProductTypeDTO>> GetTypes(
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<List<ProductTypeDTO>>(
                       "api/tags/types",
                       cancellationToken)
                   ?? new();
        }

        public async Task<bool> CreateType(
            CreateProductTypeDTO dto,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "api/tags/types")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для создания типа.");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateType(
            int typeId,
            CreateProductTypeDTO dto,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/tags/types/{typeId}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для редактирования типа.");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteType(
            int typeId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/tags/types/{typeId}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для удаления типа.");
            }

            return response.IsSuccessStatusCode;
        }

        // ================= TAGS =================

        public async Task<List<TagDTO>> GetTags(
            CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetFromJsonAsync<List<TagDTO>>(
                       "api/tags/Tags",
                       cancellationToken)
                   ?? new();
        }

        public async Task<bool> CreateTag(
            TagDTO dto,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "api/tags/Tags")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для создания тега.");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTag(
            int tagId,
            TagDTO dto,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/tags/Tags/{tagId}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для редактирования тега.");
            }

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTag(
            int tagId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/tags/Tags/{tagId}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для удаления тега.");
            }

            return response.IsSuccessStatusCode;
        }
    }
}
