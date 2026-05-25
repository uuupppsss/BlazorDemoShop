using LibDemoShop;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace BlazorDemoShop.Services
{
    public class ApiReviewsService
    {
        private readonly HttpClient _httpClient;
        public ApiReviewsService(HttpClient httpClient)
        {
            _httpClient=httpClient;
        }

        public async Task<List<ReviewResponseDto>> GetProductReviews(int productId)
        {
            return await _httpClient
                .GetFromJsonAsync<List<ReviewResponseDto>>($"api/reviews/product/{productId}")
                ?? new List<ReviewResponseDto>();
        }

        public async Task<bool> CreateReview(int productId, CreateUpdateReviewDto dto, string? token, CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"api/reviews/{productId}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для добавления отзыва.");
            }

            if (!response.IsSuccessStatusCode)
            {
                //var message = await response.Content.ReadAsStringAsync(cancellationToken);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteReview(int reviewId, string? token, CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"api/reviews/{reviewId}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для удаления отзыва.");
            }

            if (!response.IsSuccessStatusCode)
            {
                //var message = await response.Content.ReadAsStringAsync(cancellationToken);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateReview(int reviewId, CreateUpdateReviewDto dto, string? token, CancellationToken cancellationToken = default)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"api/reviews/{reviewId}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для редактирования отзыва.");
            }

            if (!response.IsSuccessStatusCode)
            {
                //var message = await response.Content.ReadAsStringAsync(cancellationToken);
                return false;
            }

            return true;
        }
    }
}
