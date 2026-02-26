using LibDemoShop;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace BlazorDemoShop.Services
{
    public class ApiAuthClient
    {
        private readonly HttpClient _httpClient;

        public ApiAuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResponseDTO> SignInAsync(LoginDTO request, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync("api/auth/signin", request, cancellationToken);
            return await ReadAuthResponseAsync(response, cancellationToken);
        }

        public async Task<AuthResponseDTO> SignUpAsync(CreateUserDTO request, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.PostAsJsonAsync("api/auth/signup", request, cancellationToken);
            return await ReadAuthResponseAsync(response, cancellationToken);
        }

        public async Task<UserInfoDTO?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/auth/user/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserInfoDTO>(cancellationToken: cancellationToken);
        }

        public async Task<AuthResponseDTO> LogoutAsync(string? token, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return await ReadAuthResponseAsync(response, cancellationToken);
        }

        private static async Task<AuthResponseDTO> ReadAuthResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(cancellationToken: cancellationToken);

            if (authResponse != null)
            {
                if (!response.IsSuccessStatusCode)
                {
                    authResponse.Success = false;
                }

                return authResponse;
            }

            return new AuthResponseDTO
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode
                    ? "Операция выполнена успешно"
                    : "Ошибка при обращении к API"
            };
        }
    }
}
