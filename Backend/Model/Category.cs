namespace Backend.Model;

public sealed record Category(
    int Id,
    string Name,
    string Slug,
    string Description);
