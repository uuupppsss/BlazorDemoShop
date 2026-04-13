using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private const string ActiveStatusTitle = "Активный";
        private const string CompletedStatusTitle = "Завершен";
        private const string CancelledStatusTitle = "Отменен";
        private const string LegacyAcceptedStatusTitle = "Принят";

        private readonly DemoShopDbContext _dbContext;

        public OrdersController(DemoShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<OrderDTO>> CreateOrder(CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var basketItems = await _dbContext.BasketItems
                .Where(x => x.UserId == userId)
                .Include(x => x.Product)
                .ToListAsync(cancellationToken);

            if (basketItems.Count == 0)
            {
                return BadRequest("Корзина пуста.");
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var statusesByTitle = await EnsureDefaultStatusesAsync(cancellationToken);
            var activeStatus = statusesByTitle[ActiveStatusTitle];

            var order = new Order
            {
                CreateDate = DateTime.Now,
                FullCost = basketItems.Sum(x => x.Product.Price * x.Count),
                StatusId = activeStatus.Id,
                UserId = userId
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var orderItems = basketItems.Select(x => new OrderItem
            {
                Count = x.Count,
                OrdeId = order.Id,
                ProductId = x.ProductId
            });

            _dbContext.OrderItems.AddRange(orderItems);
            _dbContext.BasketItems.RemoveRange(basketItems);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await BuildOrdersQuery(userId)
                .FirstAsync(x => x.Id == order.Id, cancellationToken);

            return Ok(response);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<List<OrderDTO>>> GetMyOrders(
            [FromQuery] int? orderId = null,
            [FromQuery] DateTime? createdDate = null,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var orders = await ApplyOrderSearchFilters(
                    BuildOrdersQuery(userId),
                    orderId,
                    createdDate)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync(cancellationToken);

            return Ok(orders);
        }

        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<OrderDTO>>> GetAllOrders(
            [FromQuery] int? orderId = null,
            [FromQuery] DateTime? createdDate = null,
            [FromQuery] string? userQuery = null,
            CancellationToken cancellationToken = default)
        {
            var orders = await ApplyOrderSearchFilters(
                    BuildOrdersQuery(),
                    orderId,
                    createdDate,
                    userQuery)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync(cancellationToken);

            return Ok(orders);
        }

        [HttpGet("statuses")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<OrderStatusDTO>>> GetOrderStatuses(CancellationToken cancellationToken = default)
        {
            await EnsureDefaultStatusesAsync(cancellationToken);

            var statuses = await _dbContext.OrderStatuses
                .AsNoTracking()
                .OrderBy(x => x.Title)
                .Select(x => new OrderStatusDTO
                {
                    Id = x.Id,
                    Title = x.Title
                })
                .ToListAsync(cancellationToken);

            return Ok(statuses);
        }

        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "user")]
        public async Task<ActionResult<OrderDTO>> CancelOrderByUser(
            int id,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var statusesByTitle = await EnsureDefaultStatusesAsync(cancellationToken);
            var cancelledStatus = statusesByTitle[CancelledStatusTitle];

            var order = await _dbContext.Orders
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

            if (order is null)
            {
                return NotFound("Заказ не найден.");
            }

            if (!CanCancelByUser(order.Status.Title))
            {
                return BadRequest("Пользователь может отменить только активный или принятый заказ.");
            }

            var currentKind = ResolveStatusKind(order.Status.Title);
            var transitionError = await ApplyOrderStatusSideEffectsAsync(
                order.Id,
                currentKind,
                OrderStatusKind.Cancelled,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(transitionError))
            {
                return BadRequest(transitionError);
            }

            order.StatusId = cancelledStatus.Id;
            order.RecieveDate = null;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await BuildOrdersQuery(userId)
                .FirstAsync(x => x.Id == order.Id, cancellationToken);

            return Ok(response);
        }

        [HttpPut("{id:int}/status")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<OrderDTO>> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusDTO request,
            CancellationToken cancellationToken = default)
        {
            if (request.OrderId > 0 && request.OrderId != id)
            {
                return BadRequest("Id заказа в теле запроса не совпадает с id в URL.");
            }

            if (request.StatusId <= 0)
            {
                return BadRequest("Некорректный статус заказа.");
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            await EnsureDefaultStatusesAsync(cancellationToken);

            var order = await _dbContext.Orders
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (order is null)
            {
                return NotFound("Заказ не найден.");
            }

            var currentKind = ResolveStatusKind(order.Status.Title);
            if (currentKind is OrderStatusKind.Completed or OrderStatusKind.Cancelled)
            {
                return BadRequest("Нельзя изменить статус завершенного или отмененного заказа.");
            }

            var status = await _dbContext.OrderStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.StatusId, cancellationToken);

            if (status is null)
            {
                return NotFound("Статус заказа не найден.");
            }

            var targetKind = ResolveStatusKind(status.Title);
            var transitionError = await ApplyOrderStatusSideEffectsAsync(
                order.Id,
                currentKind,
                targetKind,
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(transitionError))
            {
                return BadRequest(transitionError);
            }

            order.StatusId = status.Id;

            if (targetKind == OrderStatusKind.Completed)
            {
                order.RecieveDate = request.RecieveDate ?? DateTime.Now;
            }
            else if (request.RecieveDate.HasValue)
            {
                order.RecieveDate = request.RecieveDate.Value;
            }
            else
            {
                order.RecieveDate = null;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await BuildOrdersQuery()
                .FirstAsync(x => x.Id == order.Id, cancellationToken);

            return Ok(response);
        }

        [HttpGet("reports/sales-by-tags")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<SalesByTagReportDTO>> GetSalesByTagsReport(
            [FromQuery] DateTime dateFrom,
            [FromQuery] DateTime dateTo,
            CancellationToken cancellationToken = default)
        {
            if (!TryBuildReportPeriod(dateFrom, dateTo, out var periodFrom, out var periodTo, out var periodToExclusive, out var periodError))
            {
                return BadRequest(periodError);
            }

            var completedStatusIds = await GetCompletedStatusIdsAsync(cancellationToken);
            if (completedStatusIds.Length == 0)
            {
                return Ok(new SalesByTagReportDTO
                {
                    PeriodFrom = periodFrom,
                    PeriodTo = periodTo
                });
            }

            var completedOrderItemsQuery = _dbContext.OrderItems
                .AsNoTracking()
                .Where(x =>
                    completedStatusIds.Contains(x.Order.StatusId)
                    && x.Order.RecieveDate.HasValue
                    && x.Order.RecieveDate.Value >= periodFrom
                    && x.Order.RecieveDate.Value < periodToExclusive);

            var totalSoldItems = await completedOrderItemsQuery
                .SumAsync(x => (int?)x.Count, cancellationToken) ?? 0;

            var rawItems = await completedOrderItemsQuery
                .SelectMany(x => x.Product.ProductTags.Select(pt => new
                {
                    pt.TagId,
                    TagTitle = pt.Tag.Title,
                    TagTypeTitle = pt.Tag.Type.Title,
                    ProductId = x.ProductId,
                    SoldItems = x.Count
                }))
                .ToListAsync(cancellationToken);

            var items = rawItems
                .GroupBy(x => new { x.TagId, x.TagTitle, x.TagTypeTitle })
                .Select(g => new SalesByTagReportItemDTO
                {
                    TagId = g.Key.TagId,
                    TagTitle = g.Key.TagTitle,
                    TagTypeTitle = g.Key.TagTypeTitle,
                    SoldItems = g.Sum(x => x.SoldItems),
                    DistinctProducts = g.Select(x => x.ProductId).Distinct().Count()
                })
                .OrderByDescending(x => x.SoldItems)
                .ThenBy(x => x.TagTypeTitle)
                .ThenBy(x => x.TagTitle)
                .ToList();

            var topProducts = await completedOrderItemsQuery
                .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Count })
                .Select(g => new SalesByTagSoldProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ProductStockCount = g.Key.Count,
                    SoldItems = g.Sum(x => x.Count),
                    Revenue = g.Sum(x => x.Product.Price * x.Count)
                })
                .OrderByDescending(x => x.SoldItems)
                .ThenBy(x => x.ProductName)
                .Take(3)
                .ToListAsync(cancellationToken);

            return Ok(new SalesByTagReportDTO
            {
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                TotalSoldItems = totalSoldItems,
                TopProducts = topProducts,
                Items = items
            });
        }

        [HttpGet("reports/sales-by-tags/{tagId:int}/products")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<SalesByTagProductsReportDTO>> GetSoldProductsByTag(
            int tagId,
            [FromQuery] DateTime dateFrom,
            [FromQuery] DateTime dateTo,
            CancellationToken cancellationToken = default)
        {
            if (tagId <= 0)
            {
                return BadRequest("Некорректный тег.");
            }

            if (!TryBuildReportPeriod(dateFrom, dateTo, out var periodFrom, out var periodTo, out var periodToExclusive, out var periodError))
            {
                return BadRequest(periodError);
            }

            var tag = await _dbContext.Tags
                .AsNoTracking()
                .Where(x => x.Id == tagId)
                .Select(x => new { x.Id, x.Title })
                .FirstOrDefaultAsync(cancellationToken);

            if (tag is null)
            {
                return NotFound("Тег не найден.");
            }

            var completedStatusIds = await GetCompletedStatusIdsAsync(cancellationToken);
            if (completedStatusIds.Length == 0)
            {
                return Ok(new SalesByTagProductsReportDTO
                {
                    TagId = tag.Id,
                    TagTitle = tag.Title,
                    PeriodFrom = periodFrom,
                    PeriodTo = periodTo
                });
            }

            var soldProducts = await _dbContext.OrderItems
                .AsNoTracking()
                .Where(x =>
                    completedStatusIds.Contains(x.Order.StatusId)
                    && x.Order.RecieveDate.HasValue
                    && x.Order.RecieveDate.Value >= periodFrom
                    && x.Order.RecieveDate.Value < periodToExclusive
                    && x.Product.ProductTags.Any(pt => pt.TagId == tagId))
                .GroupBy(x => new { x.ProductId, x.Product.Name, x.Product.Count })
                .Select(g => new SalesByTagSoldProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ProductStockCount = g.Key.Count,
                    SoldItems = g.Sum(x => x.Count),
                    Revenue = g.Sum(x => x.Product.Price * x.Count)
                })
                .OrderByDescending(x => x.SoldItems)
                .ThenBy(x => x.ProductName)
                .ToListAsync(cancellationToken);

            return Ok(new SalesByTagProductsReportDTO
            {
                TagId = tag.Id,
                TagTitle = tag.Title,
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                TotalSoldItems = soldProducts.Sum(x => x.SoldItems),
                Items = soldProducts
            });
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "user,admin")]
        public async Task<ActionResult<OrderDTO>> GetOrderById(int id, CancellationToken cancellationToken = default)
        {
            var isAdmin = User.IsInRole("admin");

            if (!TryGetCurrentUserId(out var userId) && !isAdmin)
            {
                return Unauthorized();
            }

            var ordersQuery = isAdmin
                ? BuildOrdersQuery()
                : BuildOrdersQuery(userId);

            var order = await ordersQuery
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (order is null)
            {
                return NotFound("Заказ не найден.");
            }

            return Ok(order);
        }

        private IQueryable<OrderDTO> BuildOrdersQuery(int userId)
        {
            return BuildOrdersQuery().Where(x => x.UserId == userId);
        }

        private IQueryable<OrderDTO> BuildOrdersQuery()
        {
            return _dbContext.Orders
                .AsNoTracking()
                .Select(x => new OrderDTO
                {
                    Id = x.Id,
                    CreateDate = x.CreateDate,
                    FullCost = x.FullCost,
                    RecieveDate = x.RecieveDate,
                    StatusId = x.StatusId,
                    StatusTitle = x.Status.Title,
                    UserId = x.UserId,
                    UserName = x.User.Username,
                    UserEmail = x.User.Email,
                    UserPhone = x.User.ContactPhone,
                    OrderItems = x.OrderItems
                        .OrderBy(i => i.Id)
                        .Select(i => new OrderItemDTO
                        {
                            Id = i.Id,
                            Count = i.Count,
                            OrderId = i.OrdeId,
                            ProductId = i.ProductId,
                            ProductName = i.Product.Name,
                            ProductPrice = i.Product.Price
                        })
                        .ToList()
                });
        }

        private static IQueryable<OrderDTO> ApplyOrderSearchFilters(
            IQueryable<OrderDTO> query,
            int? orderId,
            DateTime? createdDate,
            string? userQuery = null)
        {
            if (orderId.HasValue && orderId.Value > 0)
            {
                query = query.Where(x => x.Id == orderId.Value);
            }

            if (createdDate.HasValue)
            {
                var dateFrom = createdDate.Value.Date;
                var dateTo = dateFrom.AddDays(1);
                query = query.Where(x => x.CreateDate >= dateFrom && x.CreateDate < dateTo);
            }

            if (!string.IsNullOrWhiteSpace(userQuery))
            {
                var normalized = userQuery.Trim();
                var hasUserId = int.TryParse(normalized, out var userId) && userId > 0;

                query = query.Where(x =>
                    (hasUserId && x.UserId == userId)
                    || (!string.IsNullOrWhiteSpace(x.UserName) && x.UserName.Contains(normalized))
                    || (!string.IsNullOrWhiteSpace(x.UserEmail) && x.UserEmail.Contains(normalized))
                    || (!string.IsNullOrWhiteSpace(x.UserPhone) && x.UserPhone.Contains(normalized)));
            }

            return query;
        }

        private async Task<string?> ApplyOrderStatusSideEffectsAsync(
            int orderId,
            OrderStatusKind fromKind,
            OrderStatusKind toKind,
            CancellationToken cancellationToken)
        {
            if (fromKind == toKind)
            {
                return null;
            }

            var orderItems = await _dbContext.OrderItems
                .Where(x => x.OrdeId == orderId)
                .Include(x => x.Product)
                .ToListAsync(cancellationToken);

            if (fromKind == OrderStatusKind.Cancelled && toKind != OrderStatusKind.Cancelled)
            {
                foreach (var orderItem in orderItems)
                {
                    if (orderItem.Product.Count < orderItem.Count)
                    {
                        return $"Недостаточно остатка по товару #{orderItem.ProductId}.";
                    }
                }

                foreach (var orderItem in orderItems)
                {
                    orderItem.Product.Count -= orderItem.Count;
                }
            }
            else if (fromKind != OrderStatusKind.Cancelled && toKind == OrderStatusKind.Cancelled)
            {
                foreach (var orderItem in orderItems)
                {
                    orderItem.Product.Count += orderItem.Count;
                }
            }

            if (fromKind != OrderStatusKind.Completed && toKind == OrderStatusKind.Completed)
            {
                foreach (var orderItem in orderItems)
                {
                    orderItem.Product.TimeBought += orderItem.Count;
                }
            }
            else if (fromKind == OrderStatusKind.Completed && toKind != OrderStatusKind.Completed)
            {
                foreach (var orderItem in orderItems)
                {
                    orderItem.Product.TimeBought = Math.Max(0, orderItem.Product.TimeBought - orderItem.Count);
                }
            }

            return null;
        }

        private async Task<int[]> GetCompletedStatusIdsAsync(CancellationToken cancellationToken)
        {
            await EnsureDefaultStatusesAsync(cancellationToken);

            var statuses = await _dbContext.OrderStatuses
                .AsNoTracking()
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken);

            return statuses
                .Where(x => ResolveStatusKind(x.Title) == OrderStatusKind.Completed)
                .Select(x => x.Id)
                .Distinct()
                .ToArray();
        }

        private static bool TryBuildReportPeriod(
            DateTime dateFrom,
            DateTime dateTo,
            out DateTime periodFrom,
            out DateTime periodTo,
            out DateTime periodToExclusive,
            out string errorMessage)
        {
            periodFrom = default;
            periodTo = default;
            periodToExclusive = default;
            errorMessage = string.Empty;

            if (dateFrom == default || dateTo == default)
            {
                errorMessage = "Укажите даты начала и конца периода.";
                return false;
            }

            periodFrom = dateFrom.Date;
            periodTo = dateTo.Date;

            if (periodFrom > periodTo)
            {
                errorMessage = "Дата начала периода не может быть позже даты окончания.";
                return false;
            }

            periodToExclusive = periodTo.AddDays(1);
            return true;
        }

        private async Task<Dictionary<string, OrderStatus>> EnsureDefaultStatusesAsync(CancellationToken cancellationToken)
        {
            var defaults = new[]
            {
                ActiveStatusTitle,
                CompletedStatusTitle,
                CancelledStatusTitle
            };

            var existingStatuses = await _dbContext.OrderStatuses
                .Where(x => defaults.Contains(x.Title))
                .ToListAsync(cancellationToken);

            foreach (var title in defaults)
            {
                if (existingStatuses.Any(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var status = new OrderStatus
                {
                    Title = title
                };

                _dbContext.OrderStatuses.Add(status);
                existingStatuses.Add(status);
            }

            if (_dbContext.ChangeTracker.HasChanges())
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return existingStatuses.ToDictionary(x => x.Title, x => x, StringComparer.OrdinalIgnoreCase);
        }

        private static bool CanCancelByUser(string? statusTitle)
        {
            var normalized = statusTitle?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            var isActive = normalized.Contains("актив") || normalized.Contains("active");
            var isAccepted = normalized.Contains("принят")
                || normalized.Contains("accepted")
                || string.Equals(statusTitle, LegacyAcceptedStatusTitle, StringComparison.OrdinalIgnoreCase);

            return isActive || isAccepted;
        }

        private static OrderStatusKind ResolveStatusKind(string? statusTitle)
        {
            var normalized = statusTitle?.Trim().ToLowerInvariant() ?? string.Empty;

            if (normalized.Contains("отмен") || normalized.Contains("cancel"))
            {
                return OrderStatusKind.Cancelled;
            }

            if (normalized.Contains("заверш")
                || normalized.Contains("выполн")
                || normalized.Contains("complete")
                || normalized.Contains("done")
                || normalized.Contains("получ"))
            {
                return OrderStatusKind.Completed;
            }

            if (normalized.Contains("актив")
                || normalized.Contains("принят")
                || normalized.Contains("обраб")
                || normalized.Contains("active")
                || string.Equals(statusTitle, LegacyAcceptedStatusTitle, StringComparison.OrdinalIgnoreCase))
            {
                return OrderStatusKind.Active;
            }

            return OrderStatusKind.Active;
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out userId) && userId > 0;
        }

        private enum OrderStatusKind
        {
            Active = 1,
            Completed = 2,
            Cancelled = 3
        }
    }
}
