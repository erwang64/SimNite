namespace SimNite.Models;

public class ModProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Mod> Mods { get; set; } = new();
}
