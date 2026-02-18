using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Auth
{
    public class UserLoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
