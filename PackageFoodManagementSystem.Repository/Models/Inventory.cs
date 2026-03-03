using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageFoodManagementSystem.Repository.Models
{
    public class Inventory
    {
        [Key]
        public int Id { get; set; }

        // Link to Product
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // Link to Batch (This contains the BatchNumber)
        public int BatchId { get; set; }
        [ForeignKey("BatchId")]
        public virtual Batch? Batch { get; set; }


        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public int StockQuantity { get; set; }


    }
}