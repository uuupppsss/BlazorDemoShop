using LibDemoShop;
using System.Net;
using System.Net.Http.Json;

namespace BlazorDemoShop.Services
{
    public class ApiProductsClientService
    {
        private readonly HttpClient _httpClient;

        public ApiProductsClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResultDTO<ProductCardDTO>> GetProductsAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            var safeSkip = Math.Max(0, skip);
            var safeTake = Math.Clamp(take, 1, 40);

            var result = await _httpClient.GetFromJsonAsync<PagedResultDTO<ProductCardDTO>>(
                $"api/products?skip={safeSkip}&take={safeTake}",
                cancellationToken);

            return result ?? new PagedResultDTO<ProductCardDTO>();
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
