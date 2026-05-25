using LibDemoShop;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BlazorDemoShop.Services
{
    public class ApiFavoriteService
    {
        private readonly HttpClient _httpClient;
        public ApiFavoriteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        // Получить избранные товары
        public async Task<List<ProductCardDTO>> GetFavoritesAsync(
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    "api/SavedProducts");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Необходимо авторизоваться.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    await response.Content
                        .ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Не удалось получить избранные товары."
                        : message);
            }

            return await response.Content
                       .ReadFromJsonAsync<List<ProductCardDTO>>(
                           cancellationToken: cancellationToken)
                   ?? new List<ProductCardDTO>();
        }

        // Toggle избранного
        public async Task<bool> ToggleFavoriteAsync(
            int productId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/SavedProducts/toggle/{productId}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Необходимо авторизоваться.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    await response.Content
                        .ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Не удалось изменить избранное."
                        : message);
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<ToggleFavoriteResponse>(
                        cancellationToken: cancellationToken);

            return result?.IsFavorite ?? false;
        }

        // Проверка находится ли товар в избранном
        public async Task<bool> IsFavoriteAsync(
            int productId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            var favorites =
                await GetFavoritesAsync(
                    token,
                    cancellationToken);

            return favorites.Any(x => x.Id == productId);
        }
    }

    public class ToggleFavoriteResponse
    {
        public bool IsFavorite { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}

