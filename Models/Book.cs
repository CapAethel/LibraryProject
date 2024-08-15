using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace LibraryProject.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string? BookDescription { get; set; }
        public string? PictureUrl { get; set; }
        public int Quantity { get; set; }
    }
}
