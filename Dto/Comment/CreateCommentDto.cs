using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Dto.Comment
{
    public class CreateCommentDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required, MinLength(1)]
        public string Content { get; set; }
    }
}
