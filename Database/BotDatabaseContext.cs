using CDRB.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace CDRB.Database;

public class BotDatabaseContext : DbContext
{
    public DbSet<DatabaseReminder> Reminders => Set<DatabaseReminder>();
    public string DbPath {get;}

    public BotDatabaseContext()
    {
        DbPath = System.IO.Path.Join("Resources", "database.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}