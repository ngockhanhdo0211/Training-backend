namespace RecursiveCommentApi.DTOs.Comments;

public class CommentTreeResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentTreeResponse> Replies { get; set; } = new();
}
