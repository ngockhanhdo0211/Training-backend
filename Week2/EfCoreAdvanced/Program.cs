using Bogus;
using EfCoreAdvanced.Data;
using EfCoreAdvanced.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(120) // Tăng timeout lên 2 phút
    );
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Đăng ký Repository (Day 8)
builder.Services.AddScoped<EfCoreAdvanced.Repositories.CommentRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint Seed Data (Day 6)
app.MapPost("/api/seed", async (AppDbContext context) =>
{
    var sw = Stopwatch.StartNew();

    // Reset data (chú ý: EF Core 7+ có thể dùng ExecuteDeleteAsync nhanh hơn)
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Comments");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Posts");
    await context.Database.ExecuteSqlRawAsync("DELETE FROM Users");

    // Fake 1,000 Users
    var userFaker = new Faker<User>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.Name, f => f.Name.FullName());
    var users = userFaker.Generate(1000);

    // Fake 10,000 Posts (Mỗi user khoảng 10 bài)
    var postFaker = new Faker<Post>()
        .RuleFor(p => p.Id, f => Guid.NewGuid())
        .RuleFor(p => p.Title, f => f.Lorem.Sentence())
        .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(2))
        .RuleFor(p => p.UserId, f => f.PickRandom(users).Id);
    var posts = postFaker.Generate(10000);

    // Fake 100,000 Comments (Mỗi post khoảng 10 comment)
    var commentFaker = new Faker<Comment>()
        .RuleFor(c => c.Id, f => Guid.NewGuid())
        .RuleFor(c => c.Content, f => f.Lorem.Sentence())
        .RuleFor(c => c.PostId, f => f.PickRandom(posts).Id)
        .RuleFor(c => c.UserId, f => f.PickRandom(users).Id);
    var comments = commentFaker.Generate(100000);

    // Dùng AddRange + Batching để không làm nổ RAM/ChangeTracker
    // Insert Users
    await context.Users.AddRangeAsync(users);
    await context.SaveChangesAsync();

    // Insert Posts theo batch 5000 records
    for (int i = 0; i < posts.Count; i += 5000)
    {
        var batch = posts.Skip(i).Take(5000);
        await context.Posts.AddRangeAsync(batch);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear(); // <-- Quan trọng! Xóa tracker để khỏi tốn RAM
    }

    // Insert Comments theo batch 5000 records
    for (int i = 0; i < comments.Count; i += 5000)
    {
        var batch = comments.Skip(i).Take(5000);
        await context.Comments.AddRangeAsync(batch);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    sw.Stop();
    return Results.Ok(new
    {
        Message = "Seeding completed successfully",
        Users = users.Count,
        Posts = posts.Count,
        Comments = comments.Count,
        TimeElapsed = $"{sw.ElapsedMilliseconds} ms"
    });
});
// Endpoint Seed Replies (Day 7)
app.MapPost("/api/seed-replies", async (AppDbContext context) =>
{
    var sw = Stopwatch.StartNew();

    // Lấy 100 posts đầu tiên để làm demo đệ quy cho nhanh (đỡ phải quét 100k comment)
    var posts = await context.Posts.Take(100).ToListAsync();
    int updatedCount = 0;

    foreach (var post in posts)
    {
        var comments = await context.Comments.Where(c => c.PostId == post.Id).ToListAsync();
        if (comments.Count < 2) continue;

        // Trộn ngẫu nhiên danh sách comment
        var shuffled = comments.OrderBy(x => Guid.NewGuid()).ToList();
        
        // Đảm bảo luôn có ít nhất 1 root comment (tránh lỗi count / 3 = 0 khi list chỉ có 2 phần tử)
        int rootCount = Math.Max(1, comments.Count / 3);
        var roots = shuffled.Take(rootCount).ToList();
        var others = shuffled.Skip(rootCount).ToList();

        var possibleParents = new List<Comment>(roots);

        foreach (var c in others)
        {
            // Chọn ngẫu nhiên 1 parent từ danh sách possible parents
            var parent = possibleParents[Random.Shared.Next(possibleParents.Count)];
            c.ParentCommentId = parent.Id;
            
            // Add comment này vào possible parents để comment sau có thể reply nó (tạo nhánh sâu)
            possibleParents.Add(c);
            updatedCount++;
        }
    }

    await context.SaveChangesAsync();
    sw.Stop();

    return Results.Ok(new
    {
        Message = "Seeding replies completed",
        RepliesCreated = updatedCount,
        TimeElapsed = $"{sw.ElapsedMilliseconds} ms"
    });
});

// Endpoint Day 8: Demo N+1 Problem và AsSplitQuery
app.MapGet("/api/posts/{id}/comments", async (Guid id, EfCoreAdvanced.Repositories.CommentRepository repo) =>
{
    var sw = Stopwatch.StartNew();
    var comments = await repo.GetAllCommentsForPost(id, includeReplies: true);
    sw.Stop();

    // Để tránh lỗi vòng lặp JSON (cycle reference) khi trả về object chứa quan hệ chéo nhau,
    // ta nên map sang object vô danh (anonymous object) hoặc DTO.
    var result = comments.Select(c => new
    {
        c.Id,
        c.Content,
        RepliesCount = c.Replies.Count,
        Replies = c.Replies.Select(r => new { r.Id, r.Content, r.UserId })
    });

    return Results.Ok(new
    {
        Message = "Query completed",
        Count = comments.Count,
        TimeElapsed = $"{sw.ElapsedMilliseconds} ms",
        Data = result
    });
});

// Endpoint Day 9 & Day 10: Lấy toàn bộ cây đệ quy bằng CTE
app.MapGet("/api/posts/{id}/comments/tree", async (Guid id, EfCoreAdvanced.Repositories.CommentRepository repo) =>
{
    var sw = Stopwatch.StartNew();
    // Lấy list comment phẳng từ CTE, EF Core Change Tracker sẽ tự nối các Object References
    var allComments = await repo.GetCommentTreeAsync(id);
    sw.Stop();

    // Lọc ra các Root Comment (không có cha) để trả về (vì nó đã tự móc nối con vào thuộc tính Replies)
    var rootComments = allComments.Where(c => c.ParentCommentId == null).ToList();

    var result = rootComments.Select(c => new
    {
        c.Id,
        c.Content,
        RepliesCount = c.Replies.Count,
        // Dùng đệ quy nhẹ để map DTO (chỉ cho output, query thì đã xong rồi)
        Replies = c.Replies.Select(r => new { r.Id, r.Content, r.ParentCommentId }) 
    });

    return Results.Ok(new
    {
        Message = "CTE Recursive Tree Query completed",
        TotalComments = allComments.Count,
        RootCommentsCount = rootComments.Count,
        TimeElapsed = $"{sw.ElapsedMilliseconds} ms",
        Data = result
    });
});

// Endpoint Day 9 & Day 10: Phá đệ quy thành list phẳng
app.MapGet("/api/posts/{id}/comments/flat", async (Guid id, EfCoreAdvanced.Repositories.CommentRepository repo) =>
{
    var sw = Stopwatch.StartNew();
    var allComments = await repo.GetCommentTreeAsync(id);
    
    // Thuật toán phá đệ quy dùng Stack
    var rootComments = allComments.Where(c => c.ParentCommentId == null).ToList();
    var flatList = repo.Flatten(rootComments);
    sw.Stop();

    var result = flatList.Select(c => new
    {
        c.Id,
        c.Content,
        c.ParentCommentId // Frontend sẽ dựa vào trường này để tự dựng lại UI cây
    });

    return Results.Ok(new
    {
        Message = "CTE Flat Query completed",
        TotalComments = flatList.Count,
        TimeElapsed = $"{sw.ElapsedMilliseconds} ms",
        Data = result
    });
});

app.Run();
