using Microsoft.EntityFrameworkCore;
using RecursiveCommentApi.Data;
using RecursiveCommentApi.Entities;

namespace RecursiveCommentApi.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Comment>> GetForPostAsync(
        Guid postId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Comment> query = _context.Comments
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.Id);

        if (limit is > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Comment?> GetByIdAsync(
        Guid id,
        bool tracking,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Comment> query = _context.Comments;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(comment => comment.Id == id, cancellationToken);
    }

    public Task<bool> ParentIsValidAsync(
        Guid parentId,
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        return _context.Comments.AnyAsync(
            comment => comment.Id == parentId && comment.PostId == postId,
            cancellationToken);
    }

    public Task<bool> HasRepliesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Comments.AnyAsync(
            comment => comment.ParentCommentId == id,
            cancellationToken);
    }

    public Task<bool> UserExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users.AnyAsync(user => user.Id == id, cancellationToken);
    }

    public void Add(Comment comment) => _context.Comments.Add(comment);
    public void Remove(Comment comment) => _context.Comments.Remove(comment);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
