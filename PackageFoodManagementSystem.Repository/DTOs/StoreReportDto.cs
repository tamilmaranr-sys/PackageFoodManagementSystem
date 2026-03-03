namespace PackageFoodManagementSystem.DTOs
{
    // This is the "Tray" that holds all the report data
    public class StoreReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
    }

    // This holds the specific data for the table rows
    public class TopProductDto
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
}