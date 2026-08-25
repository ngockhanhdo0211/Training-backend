using AutoMapper;
using RecursiveCommentApi.DTOs.Comments;
using RecursiveCommentApi.DTOs.Posts;
using RecursiveCommentApi.Entities;

namespace RecursiveCommentApi.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<Post, PostResponse>();
        CreateMap<CreatePostRequest, Post>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.User, option => option.Ignore())
            .ForMember(destination => destination.Comments, option => option.Ignore());
        CreateMap<UpdatePostRequest, Post>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.User, option => option.Ignore())
            .ForMember(destination => destination.Comments, option => option.Ignore());

        CreateMap<Comment, CommentResponse>();
        CreateMap<Comment, CommentTreeResponse>()
            .ForMember(destination => destination.Replies, option => option.Ignore());
        CreateMap<CreateCommentRequest, Comment>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.Post, option => option.Ignore())
            .ForMember(destination => destination.User, option => option.Ignore())
            .ForMember(destination => destination.ParentComment, option => option.Ignore())
            .ForMember(destination => destination.Replies, option => option.Ignore());
        CreateMap<UpdateCommentRequest, Comment>()
            .ForMember(destination => destination.Id, option => option.Ignore())
            .ForMember(destination => destination.PostId, option => option.Ignore())
            .ForMember(destination => destination.Post, option => option.Ignore())
            .ForMember(destination => destination.UserId, option => option.Ignore())
            .ForMember(destination => destination.User, option => option.Ignore())
            .ForMember(destination => destination.ParentCommentId, option => option.Ignore())
            .ForMember(destination => destination.ParentComment, option => option.Ignore())
            .ForMember(destination => destination.Replies, option => option.Ignore());
    }
}
