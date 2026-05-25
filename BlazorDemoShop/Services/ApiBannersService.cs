using LibDemoShop;
using Microsoft.AspNetCore.Components.Forms;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace BlazorDemoShop.Services
{
    public class ApiBannersService
    {
        private readonly HttpClient _httpClient;


        public ApiBannersService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Banner>> GetBannersAsync()
        {
            return await _httpClient
                .GetFromJsonAsync<List<Banner>>("api/banners")
                ?? [];
        }

        public async Task<Banner> UploadBannerAsync(
        IBrowserFile file,
        string? token,
        CancellationToken cancellationToken = default)
        {
            using var content = new MultipartFormDataContent();

            var streamContent = new StreamContent(
                file.OpenReadStream(10 * 1024 * 1024, cancellationToken));

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);

            content.Add(streamContent, "file", file.Name);

            using var request =
                new HttpRequestMessage(HttpMethod.Post, "api/banners")
                {
                    Content = content
                };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response =
                await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для добавления баннера.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Не удалось загрузить баннер."
                        : message);
            }

            var createdBanner =
                await response.Content.ReadFromJsonAsync<Banner>(
                    cancellationToken: cancellationToken);

            if (createdBanner is null)
            {
                throw new InvalidOperationException(
                    "API вернул пустой ответ при создании баннера.");
            }

            return createdBanner;
        }

        /// <summary>
        /// Обновить баннер
        /// </summary>
        public async Task<Banner> UpdateBannerAsync(
            Banner banner,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Put,
                    $"api/banners/{banner.Id}")
                {
                    Content = JsonContent.Create(banner)
                };

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response =
                await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для изменения баннера.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Не удалось обновить баннер."
                        : message);
            }

            var updatedBanner =
                await response.Content.ReadFromJsonAsync<Banner>(
                    cancellationToken: cancellationToken);

            if (updatedBanner is null)
            {
                throw new InvalidOperationException(
                    "API вернул пустой ответ при обновлении баннера.");
            }

            return updatedBanner;
        }

        /// <summary>
        /// Удалить баннер
        /// </summary>
        public async Task DeleteBannerAsync(
            int id,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"api/banners/{id}");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            using var response =
                await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException(
                    "Недостаточно прав для удаления баннера.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(message)
                        ? "Не удалось удалить баннер."
                        : message);
            }
        }
    }
}
