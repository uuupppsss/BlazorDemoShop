using LibDemoShop;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace BlazorDemoShop.Services
{
    public class ApiDeliveryService
    {
        private readonly HttpClient _httpClient;
        public ApiDeliveryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<DeliveryMethodDTO>> GetActiveMethods()
        {
            return await _httpClient
                .GetFromJsonAsync<List<DeliveryMethodDTO>>("api/DeliveryMethods")
                ?? [];
        }

        public async Task<List<DeliveryMethodDTO>> GetAllMethods(string? token, CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(HttpMethod.Get, "api/DeliveryMethods/all");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить данные."
                    : message);
            }

            var orders = await response.Content.ReadFromJsonAsync<List<DeliveryMethodDTO>>(cancellationToken: cancellationToken);
            return orders ?? new List<DeliveryMethodDTO>();
        }

        public async Task Create(DeliveryMethodDTO dto, string? token, CancellationToken cancellationToken=default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/DeliveryMethods")
            {
                Content=JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить данные."
                    : message);
            }

        }

        public async Task Update(int id, DeliveryMethodDTO dto, string? token, CancellationToken cancellationToken=default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/DeliveryMethods/{id}")
            {
                Content = JsonContent.Create(dto)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить данные."
                    : message);
            }
        }
    }
}
