using RecursiveCommentApi.DTOs.Posts;
using RecursiveCommentApi.DTOs.Common;

namespace RecursiveCommentApi.Services;

public interface IPostService
{
    Task<PagedResponse<PostResponse>> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default);

    // Kết quả có thể null khi ID client gửi lên không tồn tại.
    Task<PostResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PostResponse> CreateAsync(
        CreatePostRequest request,
        CancellationToken cancellationToken = default);

    Task<PostResponse?> UpdateAsync(
        Guid id,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
