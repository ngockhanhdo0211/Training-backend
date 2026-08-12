using EfCoreAdvanced.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreAdvanced.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình cơ bản bằng Fluent API
            // Hiện tại chúng ta giữ mọi thứ đơn giản ở Day 6
            // Quan hệ phức tạp (self-referencing) sẽ thêm vào Day 7
            
            // Ví dụ: Đặt độ dài tối đa cho Name, Title
            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .HasMaxLength(100);

            modelBuilder.Entity<Post>()
                .Property(p => p.Title)
                .HasMaxLength(255);

            // Fix lỗi Multiple Cascade Paths (Lỗi kinh điển trong SQL Server)
            // SQL Server không cho phép xóa User -> dẫn đến xóa Post -> dẫn đến xóa Comment 
            // CÙNG LÚC VỚI xóa User -> dẫn đến xóa Comment (2 đường xóa cùng trỏ về bảng Comments)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình Self-referencing (Comment lồng nhau)
            // Bắt buộc dùng Restrict ở đây để tránh Multiple Cascade Paths
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
