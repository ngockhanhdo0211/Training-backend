// INNER JOIN bằng LINQ C#
List<User> users =
[
    new User(1, "An"),
    new User(2, "Bình"),
    new User(3, "Chi")
];

List<Post> posts =
[
    new Post(101, "C# cơ bản", 1),
    new Post(102, "EF Core", 1),
    new Post(103, "SQL JOIN", 2)
];

var result =
    from user in users
    join post in posts
    on user.Id equals post.UserId
    select new
    {
        UserId = user.Id,
        user.Name,
        PostId = post.Id,
        post.Title
    };

foreach (var item in result)
{
    Console.WriteLine(
        $"{item.UserId} | {item.Name} | " +
        $"{item.PostId} | {item.Title}"
    );
}

record User(int Id, string Name);

record Post(int Id, string Title, int UserId);
