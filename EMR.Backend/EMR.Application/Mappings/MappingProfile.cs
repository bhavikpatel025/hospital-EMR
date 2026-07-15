using AutoMapper;
using EMR.Application.DTOs.Auth;
using EMR.Application.DTOs.Patients;
using EMR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMR.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.RoleName));
            CreateMap<Patient, PatientListDto>();
            CreateMap<Patient, PatientDetailDto>();
            CreateMap<PatientCreateDto, Patient>();
            CreateMap<PatientUpdateDto, Patient>();
        }
    }
}
