using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PackageFoodManagementSystem.Repository.Models
{

    [Table("Cart")]
    public class Cart
    {
        public int CartId { get; set; }

        public int UserAuthenticationId { get; set; }
        public UserAuthentication UserAuthentication { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public ICollection<CartItem> CartItems { get; set; }


    }
}