using RecursiveCommentApi.DTOs.Comments;

namespace RecursiveCommentApi.Services;

public interface ICommentService
{
    Task<IReadOnlyList<CommentResponse>> GetForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<CommentTreeQueryResponse> GetTreeForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<CommentFlatQueryResponse> GetFlatForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<CommentResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CommentResponse?> CreateAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<CommentResponse?> UpdateAsync(
        Guid id,
        UpdateCommentRequest request,
        CancellationToken cancellationToken = default);

    Task<DeleteCommentResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
