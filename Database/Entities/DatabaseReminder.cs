namespace CDRB.Database.Entities;

public class DatabaseReminder
{
    public int ID {get; init;}
    public ulong GuildID {get; init;}
    public ulong ChannelID {get; init;}
    public ulong UserID {get; init;}
    public DateTime DueDate {get; init;}
    public string? Message {get; init;}
}