using Microsoft.EntityFrameworkCore;

namespace MccSoft.IntegreSqlClient.DatabaseInitialization;

public interface IUseProvider
{
    void UseProvider(DbContextOptionsBuilder options, string connectionString);
}
