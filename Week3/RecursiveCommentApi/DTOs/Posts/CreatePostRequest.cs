using System.ComponentModel.DataAnnotations;

namespace RecursiveCommentApi.DTOs.Posts;

public class CreatePostRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}
