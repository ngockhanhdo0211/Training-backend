namespace RecursiveCommentApi.DTOs.Comments;

public class CommentTreeQueryResponse
{
    public IReadOnlyList<CommentTreeResponse> Data { get; init; } =
        Array.Empty<CommentTreeResponse>();
    public int TotalComments { get; init; }
    public double ElapsedMilliseconds { get; init; }
    public bool FromCache { get; init; }
    public int DatabaseQueryCount { get; init; }
}
