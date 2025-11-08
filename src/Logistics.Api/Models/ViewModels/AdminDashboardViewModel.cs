using System.Collections.Generic;

namespace Logistics.Api.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardViewModel()
        {
            RecentOrders = new List<OrderSummary>();
            StationSummaries = new List<StationSummary>();
            MonthlyTrends = new List<TrendData>();
        }
        
        // 订单统计数据
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // 最近订单列表
        public List<OrderSummary> RecentOrders { get; set; }
        
        // 站点概览
        public List<StationSummary> StationSummaries { get; set; }
        
        // 用户统计
        public int TotalCustomers { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalStations { get; set; }
        
        // 运营趋势数据
        public List<TrendData> MonthlyTrends { get; set; }
    }
    
    public class OrderSummary
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? CreatedTime { get; set; }
    }
    
    public class StationSummary
    {
        public string? StationId { get; set; }
        public string? StationName { get; set; }
        public int PendingOrders { get; set; }
        public int TotalInventory { get; set; }
        public string? Status { get; set; }
    }
    
    public class TrendData
    {
        public string? Period { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}