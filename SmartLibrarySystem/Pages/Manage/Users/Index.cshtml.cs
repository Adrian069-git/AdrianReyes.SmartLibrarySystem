using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartLibrarySystem.Infrastructure.Data;

namespace SmartLibrarySystem.Pages.Manage.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly SmartLibraryDbContext _dbContext;

    public List<User> Users { get; set; } = new();
    public string Keyword { get; set; } = "";
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public string SortBy { get; set; } = "username";
    public string SortOrder { get; set; } = "asc";

    public IndexModel(SmartLibraryDbContext dbContext) => _dbContext = dbContext;

    public async Task OnGetAsync(int pageIndex = 1, int pageSize = 10,
        string? sortBy = "username", string? sortOrder = "asc", string? keyword = "")
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = pageSize is 5 or 10 or 20 or 50 ? pageSize : 10;
        keyword = keyword?.Trim() ?? "";
        sortBy = (sortBy ?? "username").ToLowerInvariant();
        sortOrder = (sortOrder ?? "asc").ToLowerInvariant();

        if (sortBy != "username") sortBy = "username";
        if (sortOrder != "asc" && sortOrder != "desc") sortOrder = "asc";

        IQueryable<User> query = _dbContext.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var search = keyword.ToLowerInvariant();
            query = query.Where(u => u.UserName != null && u.UserName.ToLower().Contains(search));
        }

        query = sortOrder == "desc"
            ? query.OrderByDescending(u => u.UserName)
            : query.OrderBy(u => u.UserName);

        TotalRecords = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalRecords / (double)pageSize);

        if (TotalPages > 0 && pageIndex > TotalPages)
            pageIndex = TotalPages;

        var skip = (pageIndex - 1) * pageSize;
        Users = await query.Skip(skip).Take(pageSize).ToListAsync();

        Keyword = keyword;
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortBy = sortBy;
        SortOrder = sortOrder;
    }
}
