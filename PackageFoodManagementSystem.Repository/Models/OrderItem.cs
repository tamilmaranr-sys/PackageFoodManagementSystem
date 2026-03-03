using PackageFoodManagementSystem.Repository.Models;
using System;

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace PackageFoodManagementSystem.Repository.Models

{
    [Table("OrderItem")]
    public class OrderItem

    {

        [Key]

        public int OrderItemID { get; set; }

        // Foreign Key to Orders

        [Required]

        public int OrderID { get; set; }

        // Foreign Key to Product Management

        [Required]

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Foreign Key to Batch Management

        [Required]

        public int BatchID { get; set; }

        [Required]

        public int Quantity { get; set; }

        [Required]

        public decimal UnitPrice { get; set; }

        [Required]

        public decimal Subtotal { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string? ProductNameSnapshot { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        // Navigation Properties

        [ForeignKey("OrderID")]

        public Order Order { get; set; }
    }

}
