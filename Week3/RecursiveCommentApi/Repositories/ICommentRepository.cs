using RecursiveCommentApi.Entities;

namespace RecursiveCommentApi.Repositories;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetForPostAsync(
        Guid postId,
        int? limit,
        CancellationToken cancellationToken = default);

    Task<Comment?> GetByIdAsync(
        Guid id,
        bool tracking,
        CancellationToken cancellationToken = default);

    Task<bool> ParentIsValidAsync(
        Guid parentId,
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<bool> HasRepliesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Comment comment);
    void Remove(Comment comment);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
