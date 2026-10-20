namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public interface ICategoryEntity
{
    int CategoryId { get; set; }
    Category Category { get; set; }
}
