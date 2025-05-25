using AutoMapper;
using TaskTracker.Application.DTOs;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<TaskItem, TaskReturnDto>().ReverseMap();
            CreateMap<TaskItem, TaskRequestDto>().ReverseMap();
            CreateMap<TaskReturnDto, TaskRequestDto>().ReverseMap();
            CreateMap<User, UserReturnDto>().ReverseMap();
            CreateMap<User, UserRequestDto>().ReverseMap();
            CreateMap<UserReturnDto, UserRequestDto>().ReverseMap();
        }
    }
}
