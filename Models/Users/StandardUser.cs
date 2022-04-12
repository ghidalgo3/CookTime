namespace babe_algorithms.Models.Users;

public class StandardUser
{
    public Guid ID { get; set; }

    public ApplicationUser User { get; set; }

    public string Name { get; set; }
}