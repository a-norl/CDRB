using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CDRB.EventListeners;

public class Pinboard
{

    private readonly BotSettings _botSettings;
    private readonly string[] _videoMIMEs;

    public Pinboard(BotSettings botSettings)
    {
        _botSettings = botSettings;
        _videoMIMEs = new string[] {"video/mp4", "video/webm", "video/quicktime"};
    }

    public async Task<DiscordMessageBuilder> PinnedMessageEmbedder(DiscordMessage message)
    {
        Task<DiscordMember> getMemberTask = message.Channel.Guild.GetMemberAsync(message.Author.Id);
        DiscordEmbedBuilder embedBuilder = new();
        DiscordMessageBuilder messageBuilder = new();
        IReadOnlyList<DiscordAttachment> attachments = message.Attachments;

        DiscordMember messageAuthor = await getMemberTask;
        string nickname = messageAuthor.DisplayName;
        string AvatarUrl = messageAuthor.GetAvatarUrl(DSharpPlus.ImageFormat.Auto);

        embedBuilder
            .WithAuthor(nickname, null, AvatarUrl)
            .WithColor(messageAuthor.BannerColor is null ? DiscordColor.DarkButNotBlack : messageAuthor.BannerColor.Value);

        if (attachments.Count == 1)
        {
            if(_videoMIMEs.Contains(attachments[0].MediaType))
            {
                embedBuilder.AddField("Attached Video:", $"[URL]({attachments[0].Url})");
            }
            else 
            {
                embedBuilder.WithImageUrl(attachments[0].Url);
            }
            
            embedBuilder
                .WithDescription($"{message.Content} \n\n[Jump To Message]({message.JumpLink})")
                .WithFooter($"ID: {messageAuthor.Id}")
                .WithTimestamp(message.Timestamp);
            messageBuilder.AddEmbed(embedBuilder);
        }
        else if (attachments.Count > 1)
        {
            embedBuilder.WithDescription($"{message.Content}");
            messageBuilder.AddEmbed(embedBuilder);
            foreach (DiscordAttachment multiAttachment in attachments)
            {
                if(_videoMIMEs.Contains(multiAttachment.MediaType))
                {
                    messageBuilder.AddEmbed(new DiscordEmbedBuilder().AddField("Attached Video:", $"[URL]({multiAttachment.Url})"));
                }
                else 
                {
                    messageBuilder.AddEmbed(new DiscordEmbedBuilder().WithImageUrl(multiAttachment.Url));
                }
            }
            messageBuilder.AddEmbed(new DiscordEmbedBuilder()
                .WithDescription($"[Jump To Message]({message.JumpLink})")
                .WithFooter($"ID: {messageAuthor.Id}")
                .WithTimestamp(message.Timestamp));
        }
        else
        {
            embedBuilder
                .WithDescription($"{message.Content} \n\n[Jump To Message]({message.JumpLink})")
                .WithFooter($"ID: {messageAuthor.Id}")
                .WithTimestamp(message.Timestamp);
            messageBuilder.AddEmbed(embedBuilder);
        }

        return messageBuilder;

    }

    public async Task Archiver(DiscordClient client, MessageUpdateEventArgs eventArgs)
    {
        DiscordChannel PinChannel = eventArgs.Guild.GetChannel(_botSettings.PinChannelID);
        if (PinChannel is null) { return; }
        if(!eventArgs.Message.Pinned) return;
        DiscordMessageBuilder archivedMessage = await PinnedMessageEmbedder(eventArgs.Message);

        var archiveTask = PinChannel.SendMessageAsync(archivedMessage);
        var pinList = await eventArgs.Message.Channel.GetPinnedMessagesAsync();
        if (pinList.Count > 47)
        {
            for (int i = 0; i < 4; i++)
            {
                await pinList[pinList.Count - 1 - i].UnpinAsync();
            }
        }
        await archiveTask;
    }


}