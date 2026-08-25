using System.ComponentModel.DataAnnotations;

namespace RecursiveCommentApi.DTOs.Comments;

public class UpdateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
