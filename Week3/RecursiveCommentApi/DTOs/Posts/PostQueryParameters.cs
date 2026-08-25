using System.ComponentModel.DataAnnotations;

namespace RecursiveCommentApi.DTOs.Posts;

public class PostQueryParameters
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }
    public string SortBy { get; set; } = "id";
    public bool SortDescending { get; set; }
}
