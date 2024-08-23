namespace LibraryProject.Models
{
    public class UserEditViewModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
