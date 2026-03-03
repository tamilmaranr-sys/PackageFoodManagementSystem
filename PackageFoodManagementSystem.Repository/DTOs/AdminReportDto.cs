namespace PackageFoodManagementSystem.DTOs
{
    public class AdminReportDto
    {
        public decimal LifetimeRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalActiveOrders { get; set; }
        public List<TopCustomerDto> TopCustomers { get; set; } = new List<TopCustomerDto>();
    }

    public class TopCustomerDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}