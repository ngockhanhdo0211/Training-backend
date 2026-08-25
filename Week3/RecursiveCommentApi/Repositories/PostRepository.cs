using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using RecursiveCommentApi.Data;
using RecursiveCommentApi.DTOs.Posts;
using RecursiveCommentApi.Entities;

namespace RecursiveCommentApi.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Post> Items, int TotalCount)> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Post> query = _context.Posts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            string search = parameters.Search.Trim();
            query = query.Where(post =>
                EF.Functions.Like(post.Title, $"%{search}%") ||
                EF.Functions.Like(post.Content, $"%{search}%"));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        Expression<Func<Post, object>> sortExpression =
            parameters.SortBy.Trim().ToLowerInvariant() switch
            {
                "title" => post => post.Title,
                "userid" => post => post.UserId,
                _ => post => post.Id
            };

        query = parameters.SortDescending
            ? query.OrderByDescending(sortExpression)
            : query.OrderBy(sortExpression);

        List<Post> items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Post?> GetByIdAsync(
        Guid id,
        bool tracking,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Post> query = _context.Posts;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query.SingleOrDefaultAsync(post => post.Id == id, cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Posts.AnyAsync(post => post.Id == id, cancellationToken);
    }

    public Task<bool> UserExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Users.AnyAsync(user => user.Id == id, cancellationToken);
    }

    public void Add(Post post) => _context.Posts.Add(post);
    public void Remove(Post post) => _context.Posts.Remove(post);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
