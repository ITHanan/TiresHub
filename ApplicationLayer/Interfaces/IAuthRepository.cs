using DomainLayer.Common;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Interfaces
{
    public interface IAuthRepository
    {
        // Retrive a user by their username
      //  Task<User?> GetUserByUsernameAsync(string username);

        // Checks if email is already registered in the system
        Task<bool> EmailExistsAsync(string email);

        Task<User?> GetUserByEmailAsync(string email);

        // Adds a new user to the database (registration)
        Task CreateUserAsync(User user);

        // Saves pending changes to the database and wraps the result
        Task<OperationResult<bool>> SaveChangesAsync();
    }
}
