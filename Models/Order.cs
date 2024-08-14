using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryProject.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public int Quantity { get; set; }
        [ForeignKey(nameof(Order))]
        public int UserId { get; set; }
        public User user { get; set; }
        public string OrderStatus { get; set; } // E.g., "Pending", "Completed", "Canceled"
        public DateTime OrderDate { get; set; }
    }
}
