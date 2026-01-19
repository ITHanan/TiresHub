using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IVerificationCodeRepository
    {
        Task AddAsync(VerificationCode code);

        Task<VerificationCode?> GetValidCodeAsync(
            string identifier,
            string code);

        Task InvalidateAsync(VerificationCode code);

        Task SaveChangesAsync();
    }
}
