namespace GustavoTech;

public interface IUserService
{
    bool IsValidUser(string userName, string password);
}