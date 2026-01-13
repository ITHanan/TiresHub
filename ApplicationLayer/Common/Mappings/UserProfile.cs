using ApplicationLayer.Features.Authorize.Commands.Register;
using AutoMapper;
using DomainLayer.Users;
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
                .ConstructUsing(src =>
                    new User(
                        src.Name,
                        
                        src.UserEmail,
                        src.Phone,
                        src.Role
                    ))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());// normalize email
        }   
    }
}
