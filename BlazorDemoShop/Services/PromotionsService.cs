using LibDemoShop;
using System.Net.Http.Headers;

namespace BlazorDemoShop.Services
{
    public class PromotionsService
    {
        private readonly HttpClient _httpClient;

        public PromotionsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PromotionDto>> GetAll(string? token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/promotions");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                       .ReadFromJsonAsync<List<PromotionDto>>()
                   ?? [];
        }

        public async Task<bool> Create(
            CreateUpdatePromotionDto dto,
            string? token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/promotions")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Update(
            int id,
            CreateUpdatePromotionDto dto,
            string? token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/promotions/{id}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Delete(
            int id,
            string? token)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/promotions/{id}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
    }
}
