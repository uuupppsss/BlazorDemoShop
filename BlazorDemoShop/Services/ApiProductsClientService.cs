using LibDemoShop;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace BlazorDemoShop.Services
{
    public class ApiProductsClientService
    {
        private const long MaxUploadImageSizeBytes = 10 * 1024 * 1024;
        private readonly HttpClient _httpClient;

        public ApiProductsClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResultDTO<ProductCardDTO>> GetProductsAsync(
            int skip,
            int take,
            IEnumerable<int>? tagIds = null,
            CancellationToken cancellationToken = default)
        {
            var safeSkip = Math.Max(0, skip);
            var safeTake = Math.Clamp(take, 1, 40);
            var normalizedTagIds = (tagIds ?? Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();

            var queryBuilder = new StringBuilder($"api/products?skip={safeSkip}&take={safeTake}");

            foreach (var tagId in normalizedTagIds)
            {
                queryBuilder.Append($"&tagIds={tagId}");
            }

            var result = await _httpClient.GetFromJsonAsync<PagedResultDTO<ProductCardDTO>>(
                queryBuilder.ToString(),
                cancellationToken);

            return result ?? new PagedResultDTO<ProductCardDTO>();
        }

        public async Task<List<ProductTypeDTO>> GetTagFiltersAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ProductTypeDTO>>("api/products/tag-filters", cancellationToken);
            return result ?? new List<ProductTypeDTO>();
        }

        public async Task<ProductDTO> CreateProductAsync(
            CreateProductDTO request,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products")
            {
                Content = JsonContent.Create(request)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для добавления товара.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось добавить товар."
                    : message);
            }

            var createdProduct = await response.Content.ReadFromJsonAsync<ProductDTO>(cancellationToken: cancellationToken);

            if (createdProduct is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при создании товара.");
            }

            return createdProduct;
        }

        public async Task<List<string>> UploadProductImagesAsync(
            IEnumerable<IBrowserFile> files,
            string? token,
            CancellationToken cancellationToken = default)
        {
            var selectedFiles = files.ToList();
            if (selectedFiles.Count == 0)
            {
                return new List<string>();
            }

            using var formData = new MultipartFormDataContent();

            foreach (var file in selectedFiles)
            {
                var streamContent = new StreamContent(file.OpenReadStream(MaxUploadImageSizeBytes, cancellationToken));

                if (!string.IsNullOrWhiteSpace(file.ContentType))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                }

                formData.Add(streamContent, "files", file.Name);
            }

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/products/images/upload")
            {
                Content = formData
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для загрузки картинок.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось загрузить картинки."
                    : message);
            }

            var uploadedUrls = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);
            return uploadedUrls ?? new List<string>();
        }

        public async Task<ProductDTO> UpdateProductAsync(
            int productId,
            UpdateProductDTO request,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/products/{productId}")
            {
                Content = JsonContent.Create(request)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для редактирования товара.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Товар не найден.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось обновить товар."
                    : message);
            }

            var updatedProduct = await response.Content.ReadFromJsonAsync<ProductDTO>(cancellationToken: cancellationToken);

            if (updatedProduct is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при обновлении товара.");
            }

            return updatedProduct;
        }

        public async Task<ProductDTO?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/products/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDTO>(cancellationToken: cancellationToken);
        }
    }
}
