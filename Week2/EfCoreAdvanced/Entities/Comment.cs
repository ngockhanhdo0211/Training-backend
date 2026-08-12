using System;

namespace EfCoreAdvanced.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;

        // Foreign Key
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;

        // FK cho người comment
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Quan hệ Self-referencing (Bình luận con)
        public Guid? ParentCommentId { get; set; } // Nullable vì root comment sẽ không có cha
        public Comment? ParentComment { get; set; }
        
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
