using AutoMapper;
using Platformservice.Dtos;
using Platformservice.Models;

namespace Platformservice.Profiles
{
    public class PlatformsProfile : Profile
    {
        public PlatformsProfile()
        {
            CreateMap<Platform,PlatformReadDto>();
            CreateMap<PlatformCreateDto,Platform>();
            CreateMap<PlatformReadDto,PlatformPublishDto>();
            CreateMap<Platform, GrpcPlatformModel>()
            .ForMember(dest=>dest.PlatformId, opt=>opt.MapFrom(src=>src.Id))
            .ForMember(dest=>dest.Name, opt=>opt.MapFrom(src=>src.Name))
            .ForMember(dest=>dest.Publisher, opt=>opt.MapFrom(src=>src.Publisher));
        }
    }
}