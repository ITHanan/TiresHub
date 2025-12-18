using ApplicationLayer.Features.Authorize.Commands.Register;
using AutoMapper;
using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Common.Mappings
{
    internal class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterCommand, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // we hash it manually
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.UserEmail.ToLower())); // normalize email
        }   
    }
}
