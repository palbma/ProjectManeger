namespace ProjectManager.Dto.Auth
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; }
    }
}
