using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using PoliticalPurse.Web.Models;

namespace PoliticalPurse.Web.Services
{
    public class UserService : DatabaseService
    {
        public UserService(IOptions<DatabaseOptions> options) : base(options)
        {
        }

        public async Task<User> CheckUser(string email, string password)
        {
            using (var connection = GetConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<User>("select * from \"user\" where email = @email", new {email});
            }
        }
    }
}