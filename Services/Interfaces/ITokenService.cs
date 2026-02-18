using ProjectManager.Models;

namespace ProjectManager.Services.Interfaces
{
    public interface ITokenService
    {
       string CreateToken(User user);
    }
}
