using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageFoodManagementSystem.Repository.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public required string ProductName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public required string Category { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ImageData { get; set; }

        // Added this collection to link Products to Batches
        // This resolves the red lines in Details.cshtml
        public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
        public int CategoryId { get; set; }
    }
}