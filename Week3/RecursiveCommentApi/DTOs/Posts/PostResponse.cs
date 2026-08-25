namespace RecursiveCommentApi.DTOs.Posts;
public class PostResponse
{
    public Guid Id {get; set; } 
    public string Title {get; set;} = string.Empty;
    public string Content {get; set;} = string.Empty;
    public Guid UserId {get; set;}
}