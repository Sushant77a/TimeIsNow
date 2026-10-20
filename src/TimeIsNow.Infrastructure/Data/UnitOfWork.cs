using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;

namespace TimeIsNow.Infrastructure.Data;

public sealed class UnitOfWork(TimeIsNowDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async innerCt =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(innerCt);
            var result = await operation(innerCt);
            await transaction.CommitAsync(innerCt);
            return result;
        }, ct);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async innerCt =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(innerCt);
            await operation(innerCt);
            await transaction.CommitAsync(innerCt);
        }, ct);
    }
}
