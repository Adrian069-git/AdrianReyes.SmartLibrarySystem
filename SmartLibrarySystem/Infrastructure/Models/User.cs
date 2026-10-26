namespace SmartLibrarySystem.Infrastructure.Models;
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    
    // Optional but useful in library systems
    public string Email { get; set; } = "";
    public string ContactNumber { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}