namespace TimeIsNow.Application.Interfaces.Security;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    int UserId { get; }
    string? Email { get; }
}
