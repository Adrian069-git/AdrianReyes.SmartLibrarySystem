using System;

namespace SmartLibrarySystem.Infrastructure.Models;

public class Member
{
    public Member(Guid memberId, string key, string value)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        Key = key;
        Value = value;
    }

    public Guid Id { get; set; } // Primary Key
    public Guid MemberId { get; set; } // Foreign Key
    
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}