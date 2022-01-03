namespace babe_algorithms;
public interface IUserService
{
    bool IsValidUser(string userName, string password);
}

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private string username;
    private string password;
    // inject database for user validation
    public UserService(ILogger<UserService> logger, IConfiguration configuration)
    {
        _logger = logger;
       this.username = configuration["Authentication:BasicUsername"];
       this.password = configuration["Authentication:BasicPassword"];
    }

    public bool IsValidUser(string userName, string password)
    {
        _logger.LogInformation($"Validating user [{userName}]");
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        if (userName.Equals(this.username) && password.Equals(this.password))
        {
            return true;
        }

        return false;
    }
}