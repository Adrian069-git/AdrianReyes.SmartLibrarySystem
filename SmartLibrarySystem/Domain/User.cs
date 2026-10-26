
public class User
{
    public User(string userName, DateTime dateOfBirth)
    {
        Id = Guid.NewGuid();
        UserName = userName;
        DateOfBirth = dateOfBirth;
    }

    public Guid Id { get; set; } // Primary Key

    public string UserName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public DateTime DateOfBirth { get; set; }


 
}