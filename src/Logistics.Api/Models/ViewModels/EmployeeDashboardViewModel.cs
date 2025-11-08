using System.Collections.Generic;

namespace Logistics.Api.Models.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public EmployeeDashboardViewModel()
        {
            RecentOrders = new List<EmployeeOrderSummary>();
            Reminders = new List<WorkReminder>();
            PerformanceData = new List<PerformanceData>();
        }
        
        // 快捷统计数据
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int TodayCompleted { get; set; }
        public decimal MonthlyEarnings { get; set; }
        
        // 近期订单列表
        public List<EmployeeOrderSummary> RecentOrders { get; set; }
        
        // 工作提醒
        public List<WorkReminder> Reminders { get; set; }
        
        // 业绩概览
        public List<PerformanceData> PerformanceData { get; set; }
    }
    
    public class EmployeeOrderSummary
    {
        public string? OrderId { get; set; }
        public string? CustomerInfo { get; set; }
        public string? Status { get; set; }
        public string? DueTime { get; set; }
        public string? ActionUrl { get; set; }
    }
    
    public class WorkReminder
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? DueTime { get; set; }
    }
    
    public class PerformanceData
    {
        public string? Period { get; set; }
        public int CompletedOrders { get; set; }
        public decimal Earnings { get; set; }
    }
}