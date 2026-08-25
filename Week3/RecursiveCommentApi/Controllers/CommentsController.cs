using Microsoft.AspNetCore.Mvc;
using RecursiveCommentApi.DTOs.Comments;
using RecursiveCommentApi.Services;

namespace RecursiveCommentApi.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("post/{postId:guid}")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetForPost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CommentResponse> comments =
            await _commentService.GetForPostAsync(postId, cancellationToken);

        return Ok(comments);
    }

    [HttpGet("~/api/posts/{postId:guid}/comments/tree")]
    public async Task<ActionResult<CommentTreeQueryResponse>> GetTreeForPost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        CommentTreeQueryResponse result =
            await _commentService.GetTreeForPostAsync(postId, cancellationToken);

        Response.Headers["X-Cache"] = result.FromCache ? "HIT" : "MISS";
        Response.Headers["X-Elapsed-Milliseconds"] =
            result.ElapsedMilliseconds.ToString("F3");

        return Ok(result);
    }

    [HttpGet("~/api/posts/{postId:guid}/comments/flat")]
    public async Task<ActionResult<CommentFlatQueryResponse>> GetFlatForPost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        CommentFlatQueryResponse result =
            await _commentService.GetFlatForPostAsync(postId, cancellationToken);

        Response.Headers["X-Cache"] = result.FromCache ? "HIT" : "MISS";
        Response.Headers["X-Elapsed-Milliseconds"] =
            result.ElapsedMilliseconds.ToString("F3");

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        CommentResponse? comment =
            await _commentService.GetByIdAsync(id, cancellationToken);

        return comment is null
            ? NotFound(new { Message = $"Comment with id '{id}' was not found." })
            : Ok(comment);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponse>> Create(
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        CommentResponse? created =
            await _commentService.CreateAsync(request, cancellationToken);

        if (created is null)
        {
            return BadRequest(new
            {
                Message = "Post, user, or parent comment is invalid. A parent must belong to the same post."
            });
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> Update(
        Guid id,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        CommentResponse? updated =
            await _commentService.UpdateAsync(id, request, cancellationToken);

        return updated is null
            ? NotFound(new { Message = $"Comment with id '{id}' was not found." })
            : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        DeleteCommentResult result =
            await _commentService.DeleteAsync(id, cancellationToken);

        return result switch
        {
            DeleteCommentResult.Deleted => NoContent(),
            DeleteCommentResult.HasReplies => Conflict(new
            {
                Message = "Cannot delete a comment that still has replies."
            }),
            _ => NotFound(new { Message = $"Comment with id '{id}' was not found." })
        };
    }
}
