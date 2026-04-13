namespace BlazorDemoShop.Services
{
    public static class OrderStatusUiHelper
    {
        public static string GetDisplayTitle(string? statusTitle)
        {
            return string.IsNullOrWhiteSpace(statusTitle) ? "Активный" : statusTitle;
        }

        public static string GetBadgeCssClass(string? statusTitle)
        {
            var kind = ResolveKind(statusTitle);
            return kind switch
            {
                OrderStatusKind.Active => "order-status-pill order-status-pill-active",
                OrderStatusKind.Completed => "order-status-pill order-status-pill-completed",
                OrderStatusKind.Cancelled => "order-status-pill order-status-pill-cancelled",
                _ => "order-status-pill order-status-pill-unknown"
            };
        }

        public static string GetCardCssClass(string? statusTitle)
        {
            var kind = ResolveKind(statusTitle);
            return kind switch
            {
                OrderStatusKind.Active => "order-state-active",
                OrderStatusKind.Completed => "order-state-completed",
                OrderStatusKind.Cancelled => "order-state-cancelled",
                _ => "order-state-unknown"
            };
        }

        public static bool CanCancelByUser(string? statusTitle)
        {
            return ResolveKind(statusTitle) == OrderStatusKind.Active;
        }

        private static OrderStatusKind ResolveKind(string? statusTitle)
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
                || normalized.Contains("active"))
            {
                return OrderStatusKind.Active;
            }

            return OrderStatusKind.Active;
        }

        private enum OrderStatusKind
        {
            Active = 1,
            Completed = 2,
            Cancelled = 3,
            Unknown = 4
        }
    }
}
