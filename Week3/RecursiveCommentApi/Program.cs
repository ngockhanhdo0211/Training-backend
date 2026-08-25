using Microsoft.EntityFrameworkCore;
using RecursiveCommentApi.Data;
using RecursiveCommentApi.Repositories;
using RecursiveCommentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Ghi log ra terminal. Việc này cũng tránh lỗi quyền ghi Windows Event Log
// làm che mất exception thật từ SQL Server.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddAutoMapper(_ => { }, typeof(Program).Assembly);
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1_000;
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    scope.ServiceProvider
        .GetRequiredService<AutoMapper.IMapper>()
        .ConfigurationProvider
        .AssertConfigurationIsValid();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recursive Comment API v1");
        options.DocumentTitle = "Recursive Comment API - Week 3";
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
