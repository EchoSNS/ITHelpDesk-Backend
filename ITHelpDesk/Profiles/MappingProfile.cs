using AutoMapper;
using ITHelpDesk.Domain.Department;
using ITHelpDesk.DTOs.Position;

namespace ITHelpDesk.Profiles
{
    // In your MappingProfile.cs
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Position, PositionDto>()
                .ForMember(dest => dest.SubDepartmentName, opt => opt.MapFrom(src =>
                    src.SubDepartment != null ? src.SubDepartment.SubDepartmentName : null));

            CreateMap<CreatePositionDto, Position>();
            CreateMap<UpdatePositionDto, Position>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
