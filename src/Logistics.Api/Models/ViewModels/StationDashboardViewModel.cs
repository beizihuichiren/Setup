using System.Collections.Generic;

namespace Logistics.Api.Models.ViewModels
{
    public class StationDashboardViewModel
    {
        public StationDashboardViewModel()
        {
            PendingOrders = new List<StationOrderSummary>();
            LowInventoryProducts = new List<LowInventoryProduct>();
            PendingAfterSalesTickets = new List<AfterSalesTicketSummary>();
            WeeklyTrends = new List<WeeklyTrendData>();
        }
        
        // 站点运营概览
        public int TodayOrders { get; set; }
        public int LowInventoryAlert { get; set; }
        public int AfterSalesTickets { get; set; }
        public decimal InventoryTurnover { get; set; }
        
        // 待处理订单列表
        public List<StationOrderSummary> PendingOrders { get; set; }
        
        // 低库存产品
        public List<LowInventoryProduct> LowInventoryProducts { get; set; }
        
        // 待处理售后工单
        public List<AfterSalesTicketSummary> PendingAfterSalesTickets { get; set; }
        
        // 本周运营趋势
        public List<WeeklyTrendData> WeeklyTrends { get; set; }
    }
    
    public class StationOrderSummary
    {
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductCount { get; set; }
        public string? CreatedTime { get; set; }
        public string? ActionUrl { get; set; }
    }
    
    public class LowInventoryProduct
    {
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int AlertThreshold { get; set; }
        public string? Unit { get; set; }
    }
    
    public class AfterSalesTicketSummary
    {
        public string? TicketId { get; set; }
        public string? CustomerInfo { get; set; }
        public string? Type { get; set; }
        public string? CreatedTime { get; set; }
        public string? ActionUrl { get; set; }
    }
    
    public class WeeklyTrendData
    {
        public string? Day { get; set; }
        public int OrderCount { get; set; }
        public int CompletedCount { get; set; }
    }
}