using Microsoft.AspNetCore.Mvc;
using RecursiveCommentApi.DTOs.Common;
using RecursiveCommentApi.DTOs.Posts;
using RecursiveCommentApi.Services;

namespace RecursiveCommentApi.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PostResponse>>> GetAll(
        [FromQuery] PostQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        PagedResponse<PostResponse> posts =
            await _postService.GetPagedAsync(parameters, cancellationToken);

        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        PostResponse? post =
            await _postService.GetByIdAsync(id, cancellationToken);

        if (post is null)
        {
            return NotFound(new
            {
                Message = $"Post with id '{id}' was not found."
            });
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<PostResponse>> Create(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            PostResponse createdPost =
                await _postService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdPost.Id },
                createdPost);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostResponse>> Update(
        Guid id,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        PostResponse? updatedPost;

        try
        {
            updatedPost = await _postService.UpdateAsync(
                id,
                request,
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { Message = exception.Message });
        }

        if (updatedPost is null)
        {
            return NotFound(new
            {
                Message = $"Post with id '{id}' was not found."
            });
        }

        return Ok(updatedPost);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        bool deleted =
            await _postService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                Message = $"Post with id '{id}' was not found."
            });
        }

        return NoContent();
    }
}
