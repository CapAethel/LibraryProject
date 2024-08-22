using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryProject.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int BookId { get; set; }


        public Book? Book { get; set; }

        [Required]
        public int UserId { get; set; }

        public User? user { get; set; }

        private int _quantity;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("Quantity cannot be negative");
                _quantity = value;
            }
        }

        [Required]
        [StringLength(50)]
        public required string OrderStatus { get; set; } // E.g., "Pending", "Completed", "Canceled"

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }
    }
}
