using ProjectManager.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Users
{
    public class UserUpdateDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }

        public ProjectStatus? Status { get; set; }

        public List<int>? MemberIds { get; set; }
    }
}
