using LibDemoShop;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
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
            string? nameQuery = null,
            CancellationToken cancellationToken = default)
        {
            var safeSkip = Math.Max(0, skip);
            var safeTake = Math.Clamp(take, 1, 40);
            var normalizedTagIds = (tagIds ?? Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();
            var normalizedNameQuery = nameQuery?.Trim();

            var queryBuilder = new StringBuilder($"api/products?skip={safeSkip}&take={safeTake}");

            foreach (var tagId in normalizedTagIds)
            {
                queryBuilder.Append($"&tagIds={tagId}");
            }

            if (!string.IsNullOrWhiteSpace(normalizedNameQuery))
            {
                queryBuilder.Append($"&nameQuery={Uri.EscapeDataString(normalizedNameQuery)}");
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

        public async Task<List<BasketItemDTO>> GetBasketItemsAsync(string? token, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/basket");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра корзины.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось загрузить корзину."
                    : message);
            }

            var result = await response.Content.ReadFromJsonAsync<List<BasketItemDTO>>(cancellationToken: cancellationToken);
            return result ?? new List<BasketItemDTO>();
        }

        public async Task<BasketItemDTO> AddToBasketAsync(
            int productId,
            int count,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/basket/items")
            {
                Content = JsonContent.Create(new CreateBasketItemDTO
                {
                    ProductId = productId,
                    Count = count,
                    UserId = 0
                })
            };

            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для добавления товара в корзину.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Товар не найден.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось добавить товар в корзину."
                    : message);
            }

            var basketItem = await response.Content.ReadFromJsonAsync<BasketItemDTO>(cancellationToken: cancellationToken);
            if (basketItem is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при добавлении товара в корзину.");
            }

            return basketItem;
        }

        public async Task<BasketItemDTO> UpdateBasketItemCountAsync(
            int basketItemId,
            int count,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/basket/items/{basketItemId}")
            {
                Content = JsonContent.Create(new UpdateBasketItemDTO
                {
                    Id = basketItemId,
                    Count = count
                })
            };

            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для изменения количества товара.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Позиция корзины не найдена.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось изменить количество товара."
                    : message);
            }

            var basketItem = await response.Content.ReadFromJsonAsync<BasketItemDTO>(cancellationToken: cancellationToken);
            if (basketItem is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при обновлении количества товара.");
            }

            return basketItem;
        }

        public async Task RemoveBasketItemAsync(
            int basketItemId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/basket/items/{basketItemId}");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для удаления товара из корзины.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("Позиция корзины не найдена.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось удалить товар из корзины."
                    : message);
            }
        }

        public async Task<OrderDTO> CreateOrderAsync(
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/orders");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для оформления заказа.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось оформить заказ: корзина пуста."
                    : message);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось оформить заказ."
                    : message);
            }

            var order = await response.Content.ReadFromJsonAsync<OrderDTO>(cancellationToken: cancellationToken);
            if (order is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при создании заказа.");
            }

            return order;
        }

        public async Task<OrderDTO> CancelOrderAsync(
            int orderId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/orders/{orderId}/cancel");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для отмены заказа.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var notFoundMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(notFoundMessage)
                    ? "Заказ не найден."
                    : notFoundMessage);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var badRequestMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(badRequestMessage)
                    ? "Не удалось отменить заказ."
                    : badRequestMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось отменить заказ."
                    : message);
            }

            var order = await response.Content.ReadFromJsonAsync<OrderDTO>(cancellationToken: cancellationToken);
            if (order is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при отмене заказа.");
            }

            return order;
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(
            int orderId,
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра заказа.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить данные заказа."
                    : message);
            }

            return await response.Content.ReadFromJsonAsync<OrderDTO>(cancellationToken: cancellationToken);
        }

        public async Task<List<OrderDTO>> GetMyOrdersAsync(
            string? token,
            int? orderId = null,
            DateTime? createdDate = null,
            CancellationToken cancellationToken = default)
        {
            var route = BuildOrdersSearchRoute(
                "api/orders/mine",
                orderId,
                createdDate);

            using var request = new HttpRequestMessage(HttpMethod.Get, route);
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра заказов.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить список заказов."
                    : message);
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDTO>>(cancellationToken: cancellationToken);
            return orders ?? new List<OrderDTO>();
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync(
            string? token,
            int? orderId = null,
            DateTime? createdDate = null,
            string? userQuery = null,
            CancellationToken cancellationToken = default)
        {
            var route = BuildOrdersSearchRoute(
                "api/orders/all",
                orderId,
                createdDate,
                userQuery);

            using var request = new HttpRequestMessage(HttpMethod.Get, route);
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра всех заказов.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить список всех заказов."
                    : message);
            }

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDTO>>(cancellationToken: cancellationToken);
            return orders ?? new List<OrderDTO>();
        }

        public async Task<List<OrderStatusDTO>> GetOrderStatusesAsync(
            string? token,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/orders/statuses");
            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для получения статусов заказа.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить статусы заказов."
                    : message);
            }

            var statuses = await response.Content.ReadFromJsonAsync<List<OrderStatusDTO>>(cancellationToken: cancellationToken);
            return statuses ?? new List<OrderStatusDTO>();
        }

        public async Task<SalesByTagReportDTO> GetSalesByTagsReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string? token,
            CancellationToken cancellationToken = default)
        {
            var normalizedFrom = dateFrom.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var normalizedTo = dateTo.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/orders/reports/sales-by-tags?dateFrom={normalizedFrom}&dateTo={normalizedTo}");

            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для получения отчета по продажам.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var badRequestMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(badRequestMessage)
                    ? "Некорректный период отчета."
                    : badRequestMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить отчет по продажам."
                    : message);
            }

            var report = await response.Content.ReadFromJsonAsync<SalesByTagReportDTO>(cancellationToken: cancellationToken);
            if (report is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при загрузке отчета.");
            }

            return report;
        }

        public async Task<SalesByTagProductsReportDTO> GetSoldProductsByTagAsync(
            int tagId,
            DateTime dateFrom,
            DateTime dateTo,
            string? token,
            CancellationToken cancellationToken = default)
        {
            if (tagId <= 0)
            {
                throw new InvalidOperationException("Некорректный тег.");
            }

            var normalizedFrom = dateFrom.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var normalizedTo = dateTo.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/orders/reports/sales-by-tags/{tagId}/products?dateFrom={normalizedFrom}&dateTo={normalizedTo}");

            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для просмотра товаров по тегу.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Тег не найден."
                    : message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Некорректный запрос для отчета по товарам."
                    : message);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось получить список товаров по тегу."
                    : message);
            }

            var report = await response.Content.ReadFromJsonAsync<SalesByTagProductsReportDTO>(cancellationToken: cancellationToken);
            if (report is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при загрузке товаров по тегу.");
            }

            return report;
        }

        public async Task<OrderDTO> UpdateOrderStatusAsync(
            int orderId,
            int statusId,
            string? token,
            DateTime? recieveDate = null,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/orders/{orderId}/status")
            {
                Content = JsonContent.Create(new UpdateOrderStatusDTO
                {
                    OrderId = orderId,
                    StatusId = statusId,
                    RecieveDate = recieveDate
                })
            };

            AppendAuthHeader(request, token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new InvalidOperationException("Недостаточно прав для изменения статуса заказа.");
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var notFoundMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(notFoundMessage)
                    ? "Заказ или статус не найден."
                    : notFoundMessage);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var badRequestMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(badRequestMessage)
                    ? "Не удалось изменить статус заказа."
                    : badRequestMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
                    ? "Не удалось изменить статус заказа."
                    : message);
            }

            var order = await response.Content.ReadFromJsonAsync<OrderDTO>(cancellationToken: cancellationToken);
            if (order is null)
            {
                throw new InvalidOperationException("API вернул пустой ответ при изменении статуса заказа.");
            }

            return order;
        }

        private static string BuildOrdersSearchRoute(
            string basePath,
            int? orderId,
            DateTime? createdDate,
            string? userQuery = null)
        {
            var queryParts = new List<string>();

            if (orderId.HasValue && orderId.Value > 0)
            {
                queryParts.Add($"orderId={orderId.Value}");
            }

            if (createdDate.HasValue)
            {
                var dateValue = createdDate.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                queryParts.Add($"createdDate={dateValue}");
            }

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                queryParts.Add($"userQuery={Uri.EscapeDataString(userQuery.Trim())}");
            }

            if (queryParts.Count == 0)
            {
                return basePath;
            }

            return $"{basePath}?{string.Join("&", queryParts)}";
        }

        private static void AppendAuthHeader(HttpRequestMessage request, string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
