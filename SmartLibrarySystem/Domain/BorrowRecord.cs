using System;

namespace SmartLibrarySystem.Infrastructure.Models;

public class BorrowRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    //  Foreign Keys
    public int BookId { get; set; }
    public Guid MemberId { get; set; }

    //  Dates
    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    //  Navigation Properties
    public Book? Book { get; set; }
    public Member? Member { get; set; }
}