using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using RecursiveCommentApi.DTOs.Comments;
using RecursiveCommentApi.Entities;
using RecursiveCommentApi.Repositories;

namespace RecursiveCommentApi.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IMapper mapper,
        IMemoryCache cache)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IReadOnlyList<CommentResponse>> GetForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Comment> comments =
            await _commentRepository.GetForPostAsync(postId, 100, cancellationToken);

        return _mapper.Map<List<CommentResponse>>(comments);
    }

    public async Task<CommentTreeQueryResponse> GetTreeForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string cacheKey = GetTreeCacheKey(postId);

        if (_cache.TryGetValue(
            cacheKey,
            out IReadOnlyList<CommentTreeResponse>? cachedTree) &&
            cachedTree is not null)
        {
            stopwatch.Stop();
            return new CommentTreeQueryResponse
            {
                Data = cachedTree,
                TotalComments = CountTreeNodes(cachedTree),
                ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                FromCache = true,
                DatabaseQueryCount = 0
            };
        }

        IReadOnlyList<Comment> comments =
            await _commentRepository.GetForPostAsync(postId, null, cancellationToken);

        Dictionary<Guid, Guid?> parentById = comments.ToDictionary(
            comment => comment.Id,
            comment => comment.ParentCommentId);

        HashSet<Guid> cyclicIds = FindCyclicIds(parentById);

        Dictionary<Guid, CommentTreeResponse> nodes = comments.ToDictionary(
            comment => comment.Id,
            comment => _mapper.Map<CommentTreeResponse>(comment));

        List<CommentTreeResponse> roots = new();

        foreach (Comment comment in comments)
        {
            CommentTreeResponse node = nodes[comment.Id];

            if (!cyclicIds.Contains(comment.Id) &&
                comment.ParentCommentId is Guid parentId &&
                nodes.TryGetValue(parentId, out CommentTreeResponse? parent))
            {
                parent.Replies.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        _cache.Set(
            cacheKey,
            roots,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(1),
                Size = 1
            });

        stopwatch.Stop();
        return new CommentTreeQueryResponse
        {
            Data = roots,
            TotalComments = comments.Count,
            ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            FromCache = false,
            DatabaseQueryCount = 1
        };
    }

    public async Task<CommentFlatQueryResponse> GetFlatForPostAsync(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        CommentTreeQueryResponse treeResult =
            await GetTreeForPostAsync(postId, cancellationToken);

        List<CommentResponse> flattened = new();
        Stack<CommentTreeResponse> stack = new();

        for (int i = treeResult.Data.Count - 1; i >= 0; i--)
        {
            stack.Push(treeResult.Data[i]);
        }

        while (stack.Count > 0)
        {
            CommentTreeResponse current = stack.Pop();
            flattened.Add(new CommentResponse
            {
                Id = current.Id,
                Content = current.Content,
                PostId = current.PostId,
                UserId = current.UserId,
                ParentCommentId = current.ParentCommentId
            });

            for (int i = current.Replies.Count - 1; i >= 0; i--)
            {
                stack.Push(current.Replies[i]);
            }
        }

        stopwatch.Stop();
        return new CommentFlatQueryResponse
        {
            Data = flattened,
            TotalComments = flattened.Count,
            ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
            FromCache = treeResult.FromCache,
            DatabaseQueryCount = treeResult.DatabaseQueryCount
        };
    }

    public async Task<CommentResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Comment? comment =
            await _commentRepository.GetByIdAsync(id, false, cancellationToken);

        return _mapper.Map<CommentResponse?>(comment);
    }

    public async Task<CommentResponse?> CreateAsync(
        CreateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        bool postExists =
            await _postRepository.ExistsAsync(request.PostId, cancellationToken);
        bool userExists =
            await _commentRepository.UserExistsAsync(request.UserId, cancellationToken);

        if (!postExists || !userExists)
        {
            return null;
        }

        if (request.ParentCommentId is Guid parentId &&
            !await _commentRepository.ParentIsValidAsync(
                parentId,
                request.PostId,
                cancellationToken))
        {
            return null;
        }

        Comment comment = _mapper.Map<Comment>(request);
        comment.Id = Guid.NewGuid();
        comment.Content = comment.Content.Trim();

        _commentRepository.Add(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);
        InvalidateTreeCache(comment.PostId);

        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task<CommentResponse?> UpdateAsync(
        Guid id,
        UpdateCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        Comment? comment =
            await _commentRepository.GetByIdAsync(id, true, cancellationToken);

        if (comment is null)
        {
            return null;
        }

        _mapper.Map(request, comment);
        comment.Content = comment.Content.Trim();
        await _commentRepository.SaveChangesAsync(cancellationToken);
        InvalidateTreeCache(comment.PostId);

        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task<DeleteCommentResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Comment? comment =
            await _commentRepository.GetByIdAsync(id, true, cancellationToken);

        if (comment is null)
        {
            return DeleteCommentResult.NotFound;
        }

        if (await _commentRepository.HasRepliesAsync(id, cancellationToken))
        {
            return DeleteCommentResult.HasReplies;
        }

        _commentRepository.Remove(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);
        InvalidateTreeCache(comment.PostId);
        return DeleteCommentResult.Deleted;
    }

    private static HashSet<Guid> FindCyclicIds(
        IReadOnlyDictionary<Guid, Guid?> parentById)
    {
        HashSet<Guid> processed = new();
        HashSet<Guid> cyclicIds = new();

        foreach (Guid startId in parentById.Keys)
        {
            if (processed.Contains(startId))
            {
                continue;
            }

            List<Guid> path = new();
            Dictionary<Guid, int> indexInPath = new();
            Guid currentId = startId;

            while (!processed.Contains(currentId) &&
                   parentById.ContainsKey(currentId))
            {
                if (indexInPath.TryGetValue(currentId, out int cycleStartIndex))
                {
                    for (int i = cycleStartIndex; i < path.Count; i++)
                    {
                        cyclicIds.Add(path[i]);
                    }

                    break;
                }

                indexInPath[currentId] = path.Count;
                path.Add(currentId);

                Guid? parentId = parentById[currentId];
                if (parentId is null)
                {
                    break;
                }

                currentId = parentId.Value;
            }

            foreach (Guid id in path)
            {
                processed.Add(id);
            }
        }

        return cyclicIds;
    }

    private static int CountTreeNodes(IReadOnlyList<CommentTreeResponse> roots)
    {
        int count = 0;
        Stack<CommentTreeResponse> stack = new();

        for (int i = roots.Count - 1; i >= 0; i--)
        {
            stack.Push(roots[i]);
        }

        while (stack.Count > 0)
        {
            CommentTreeResponse current = stack.Pop();
            count++;

            foreach (CommentTreeResponse reply in current.Replies)
            {
                stack.Push(reply);
            }
        }

        return count;
    }

    private static string GetTreeCacheKey(Guid postId) => $"comment-tree:{postId}";

    private void InvalidateTreeCache(Guid postId)
    {
        _cache.Remove(GetTreeCacheKey(postId));
    }
}
