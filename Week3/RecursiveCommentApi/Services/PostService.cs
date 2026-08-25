using AutoMapper;
using RecursiveCommentApi.DTOs.Common;
using RecursiveCommentApi.DTOs.Posts;
using RecursiveCommentApi.Entities;
using RecursiveCommentApi.Repositories;

namespace RecursiveCommentApi.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;
    private readonly IMapper _mapper;

    public PostService(IPostRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PostResponse>> GetPagedAsync(
        PostQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        (IReadOnlyList<Post> posts, int totalCount) =
            await _repository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResponse<PostResponse>
        {
            Items = _mapper.Map<List<PostResponse>>(posts),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PostResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Post? post = await _repository.GetByIdAsync(id, false, cancellationToken);
        return _mapper.Map<PostResponse?>(post);
    }

    public async Task<PostResponse> CreateAsync(
        CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _repository.UserExistsAsync(request.UserId, cancellationToken))
        {
            throw new ArgumentException("UserId does not reference an existing user.");
        }

        Post post = _mapper.Map<Post>(request);
        post.Id = Guid.NewGuid();
        post.Title = post.Title.Trim();
        post.Content = post.Content.Trim();

        _repository.Add(post);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PostResponse>(post);
    }

    public async Task<PostResponse?> UpdateAsync(
        Guid id,
        UpdatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        Post? post = await _repository.GetByIdAsync(id, true, cancellationToken);

        if (post is null)
        {
            return null;
        }

        if (!await _repository.UserExistsAsync(request.UserId, cancellationToken))
        {
            throw new ArgumentException("UserId does not reference an existing user.");
        }

        _mapper.Map(request, post);
        post.Title = post.Title.Trim();
        post.Content = post.Content.Trim();

        await _repository.SaveChangesAsync(cancellationToken);
        return _mapper.Map<PostResponse>(post);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Post? post = await _repository.GetByIdAsync(id, true, cancellationToken);

        if (post is null)
        {
            return false;
        }

        _repository.Remove(post);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
