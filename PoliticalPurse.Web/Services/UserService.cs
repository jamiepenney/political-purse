using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PoliticalPurse.Web.Models;

namespace PoliticalPurse.Web.Services
{
    public class UserService : DatabaseService
    {
        private readonly ILogger<UserService> _logger;
        public UserService(IOptions<DatabaseOptions> options, ILogger<UserService> logger) : base(options)
        {
            _logger = logger;
        }

        public async Task<User> CheckUser(string email, string password)
        {
            using (var connection = GetConnection())
            {
                var user = await connection.QueryFirstOrDefaultAsync<User>("select * from \"user\" where email = @email", new {email});

                if (user == null)
                {
                    _logger.LogWarning("Login failed for '{model.Email}' - email doesn't exist");
                    return null;
                }

                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Login succeeded for '{model.Email}'");
                    return user;
                }

                _logger.LogWarning("Login failed for '{model.Email}' - incorrect password");
                return null;
            }
        }
    }
}