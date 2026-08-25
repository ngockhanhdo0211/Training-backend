using System.ComponentModel.DataAnnotations;

namespace RecursiveCommentApi.DTOs.Comments;

public class CreateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;

    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
}
