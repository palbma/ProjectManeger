using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Auth
{
    public class UserRegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(2)]
        public string FirstName { get; set; }

        [Required, MinLength(2)]
        public string LastName { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        public string? RoleName { get; set; } = "member";
    }
}
