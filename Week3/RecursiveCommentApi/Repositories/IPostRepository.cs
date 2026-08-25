using RecursiveCommentApi.Entities;
using RecursiveCommentApi.DTOs.Posts;

namespace RecursiveCommentApi.Repositories;

public interface IPostRepository
{
    Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<Post?> GetByIdAsync(
        Guid id,
        bool tracking,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Post post);
    void Remove(Post post);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
