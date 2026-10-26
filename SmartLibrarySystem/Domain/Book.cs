using System;

namespace SmartLibrarySystem.Infrastructure.Models;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string ISBN { get; set; } = "";

    //  Relationship
    public int AuthorId { get; set; }
    public Author? Author { get; set; }

    //  Inventory
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //  Borrow History
    public ICollection<BorrowRecord>? BorrowRecords { get; set; }
}