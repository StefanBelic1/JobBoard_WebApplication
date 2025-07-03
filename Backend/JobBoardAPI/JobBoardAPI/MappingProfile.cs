using AutoMapper;
using JobBoardAPI.Model;
using JobBoardAPI.RestModels;

namespace JobBoardAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Job, JobREST>().ReverseMap();
            CreateMap<Application, ApplicationREST>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(src => src.JobId))
                .ReverseMap();
            CreateMap<User, UserREST>().ReverseMap();
        }
    }
}