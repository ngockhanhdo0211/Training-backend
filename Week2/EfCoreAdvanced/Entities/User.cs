using System;
using System.Collections.Generic;

namespace EfCoreAdvanced.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation property (1 User - N Posts)
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
