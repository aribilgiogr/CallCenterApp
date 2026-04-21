using AutoMapper;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    public class Profiles : Profile
    {
        public Profiles()
        {
            CreateMap<LeadCreateDto, Lead>();
            CreateMap<Lead, LeadListItemDto>()
                .ForMember(destination => destination.AssignedUserName, option => option.MapFrom(source => source.AssignedUser != null ? $"{source.AssignedUser.FirstName} {source.AssignedUser.LastName}" : null));
        }
    }
}
