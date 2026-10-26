namespace SmartLibrarySystem.Infrastructure.Helpers;

public static class ProfileImageHelper
{
    public static string GetImagePath(string webRootPath, Guid userId)
    {
        var usersFolder = Path.Combine(webRootPath, "users");
        var userImage = Path.Combine(usersFolder, $"{userId}.png");

        return File.Exists(userImage)
            ? $"/users/{userId}.png"
            : "/users/default.png";
    }
}
