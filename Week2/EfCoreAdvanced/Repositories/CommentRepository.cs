using EfCoreAdvanced.Data;
using EfCoreAdvanced.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreAdvanced.Repositories
{
    public class CommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        // Demo N+1: Lấy tất cả comment gốc của 1 post kèm theo các reply (1 cấp)
        public async Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true)
        {
            var query = _context.Comments.Where(c => c.PostId == postId && c.ParentCommentId == null);

            if (includeReplies)
            {
                // Dùng Include để lấy cấp con (Replies). 
                // Nếu dùng Include nhiều quá (VD: .ThenInclude(r => r.Replies)) sẽ gây ra Cartesian Explosion (nhân bản dữ liệu).
                // Do đó, ta dùng AsSplitQuery() để EF Core tự tách ra thành các câu query nhỏ.
                query = query
                    .Include(c => c.Replies)
                    .ThenInclude(r => r.User) // Lấy thêm User của Reply để test N+1
                    .AsSplitQuery();
            }

            return await query.ToListAsync();
        }

        // Day 9: Dùng Raw SQL (CTE) để lấy toàn bộ cây vô hạn cấp chỉ bằng 1 query
        public async Task<List<Comment>> GetCommentTreeAsync(Guid postId)
        {
            var sql = @"
            WITH CommentTree AS (
                -- Anchor member: Lấy các comment gốc của bài viết
                SELECT * FROM Comments 
                WHERE PostId = @postId AND ParentCommentId IS NULL
                
                UNION ALL
                
                -- Recursive member: JOIN với chính CTE để lấy các cấp con
                SELECT c.* FROM Comments c
                INNER JOIN CommentTree ct ON c.ParentCommentId = ct.Id
            )
            SELECT * FROM CommentTree";

            // Dùng EF Core gọi Raw SQL
            // Lưu ý: Dữ liệu trả về từ Raw SQL mặc định phẳng, EF Core sẽ tự ráp cây nếu các entity đã được track.
            var comments = await _context.Comments
                .FromSqlRaw(sql, new Microsoft.Data.SqlClient.SqlParameter("@postId", postId))
                .ToListAsync();

            return comments;
        }

        // Day 9: Thuật toán Phá đệ quy (Flatten) dùng Stack
        public List<Comment> Flatten(IEnumerable<Comment> roots)
        {
            var result = new List<Comment>();
            var stack = new Stack<Comment>(roots);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                result.Add(current);

                // Nếu có replies, đẩy vào stack để xử lý tiếp
                foreach (var reply in current.Replies)
                {
                    stack.Push(reply);
                }
            }

            return result;
        }
    }
}
