namespace Jarvis.Models.Identity.Models.Identity
{
    public class CreateUserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Metadata { get; set; }
    }
}
