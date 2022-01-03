namespace babe_algorithms;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ISet<Recipe> Recipes { get; set; }
}