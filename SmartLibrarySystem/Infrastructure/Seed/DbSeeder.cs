using BCrypt.Net;
using SmartLibrarySystem.Infrastructure.Data;

public static class DbSeeder
{
    public static void Seed(SmartLibraryDbContext context)
    {
        var admin = context.Users.FirstOrDefault(u => u.UserName == "admin123@gmail.com");

        if (admin == null)
        {
            admin = new User("admin123@gmail.com", new DateTime(2000, 1, 1))
            {
                FirstName = "System",
                LastName = "Administrator"
            };

            context.Users.Add(admin);
            context.SaveChanges();

            context.UserLoginInfos.AddRange(
                new UserLoginInfo(admin.Id, "Password", BCrypt.Net.BCrypt.HashPassword("admin123")),
                new UserLoginInfo(admin.Id, "LoginStatus", "Active"),
                new UserLoginInfo(admin.Id, "LoginRetries", "0"),
                new UserLoginInfo(admin.Id, "role", "Admin")
            );
            context.SaveChanges();
        }
        else
        {
            var adminRole = context.UserLoginInfos.FirstOrDefault(x =>
                x.UserId == admin.Id && x.Key != null && x.Key.ToLower() == "role");
            if (adminRole == null)
                context.UserLoginInfos.Add(new UserLoginInfo(admin.Id, "role", "Admin"));
            else
                adminRole.Value = "Admin";
        }

        // Activity 11 requirement: every user should have a role record.
        var users = context.Users.ToList();
        foreach (var user in users)
        {
            var hasRole = context.UserLoginInfos.Any(x =>
                x.UserId == user.Id && x.Key != null && x.Key.ToLower() == "role");

            if (!hasRole)
                context.UserLoginInfos.Add(new UserLoginInfo(user.Id, "role",
                    user.Id == admin.Id ? "Admin" : "User"));
        }

        context.SaveChanges();
    }
}
