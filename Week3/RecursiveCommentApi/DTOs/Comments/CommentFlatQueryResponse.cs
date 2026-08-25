namespace RecursiveCommentApi.DTOs.Comments;

public class CommentFlatQueryResponse
{
    public IReadOnlyList<CommentResponse> Data { get; init; } =
        Array.Empty<CommentResponse>();
    public int TotalComments { get; init; }
    public double ElapsedMilliseconds { get; init; }
    public bool FromCache { get; init; }
    public int DatabaseQueryCount { get; init; }
}
