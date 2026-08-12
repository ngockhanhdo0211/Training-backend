using System;
using System.Collections.Generic;

namespace EfCoreAdvanced.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        // Foreign Key
        public Guid UserId { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
