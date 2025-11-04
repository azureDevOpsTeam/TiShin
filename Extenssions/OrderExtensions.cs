using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Extenssions;

public static class OrderExtensions
{
    public static string GenerateTrackingCode()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var rand = new Random();
        var randomPart = rand.Next(1000, 9999).ToString();
        return $"TS-{datePart}-{randomPart}";
    }

    public static string GetStatusColor(this OrderStatus status) => status switch
    {
        OrderStatus.Paid => "text-green-600",
        OrderStatus.Pending => "text-yellow-500",
        OrderStatus.Shipped => "text-blue-600",
        OrderStatus.Delivered => "text-emerald-600",
        OrderStatus.Canceled => "text-gray-600",
        OrderStatus.Failed => "text-red-600",
        _ => "text-gray-600"
    };

    public static string GetDisplayName(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Paid => "پرداخت شده",
            OrderStatus.Pending => "در انتظار پرداخت",
            OrderStatus.Shipped => "ارسال شده",
            OrderStatus.Delivered => "تحویل شده",
            OrderStatus.Canceled => "لغو شده",
            OrderStatus.Failed => "ناموفق",
            _ => "نامشخص"
        };
    }

    public static string GetBadgeClass(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Paid => "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
            OrderStatus.Pending => "bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400",
            OrderStatus.Shipped => "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
            OrderStatus.Delivered => "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-400",
            OrderStatus.Canceled => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300",
            OrderStatus.Failed => "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
            _ => "bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300"
        };
    }
}