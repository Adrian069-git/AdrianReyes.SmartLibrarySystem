using SmartLibrarySystem.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartLibrarySystem.Infrastructure.Data;

    public class SmartLibraryDbContext : DbContext
    {
        public SmartLibraryDbContext(DbContextOptions<SmartLibraryDbContext> options)
            : base(options)
        {
        }


        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserLoginInfo> UserLoginInfos { get; set; }
    }

