namespace PoliticalPurse.Web.Models
{
    public class User
    {
        public User() {}

        public User(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}