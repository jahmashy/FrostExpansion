namespace Frost.Server.Services.Interfaces
{
    public interface IAuthService
    {
        public bool AuthorizeUserIdentityFromEmail(string email, string token);
        public bool AuthorizeUserIdentityFromId(int id, string token);
    }
}
