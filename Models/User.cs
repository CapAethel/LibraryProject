using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LibraryProject.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public String Name { get; set; }
        public String Email { get; set; }
        [DataType(DataType.Password)]
        public String Password { get; set; }
        public Role Role { get; set; }
        public int RoleId { get; set; }
    }
}
