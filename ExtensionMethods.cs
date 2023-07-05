using DSharpPlus.Entities;
using ImageMagick;
using Serilog;

namespace CDRB;

public static class ExtensionMethods
{
    public static async Task<MagickImage> ConvertToImageAsync(this DiscordAttachment attachment, HttpClient httpClient)
    {
        var imageStream = await httpClient.GetStreamAsync(attachment.Url);
        try
        {
            return new MagickImage(imageStream);
        }
        catch (System.Exception e)
        {
            Log.Error(e, "Image conversion error:");
            return new MagickImage(new MagickColor(0,0,0),500,500);
        }
    }

    public static async Task<MagickImage> ConvertToImageAsync(this DiscordMember member, HttpClient httpClient)
    {
        var imageStream = await httpClient.GetStreamAsync(member.GetGuildAvatarUrl(DSharpPlus.ImageFormat.Auto));
        try
        {
            return new MagickImage(imageStream);
        }
        catch (System.Exception e)
        {
            Log.Error(e, "Image conversion error:");
            return new MagickImage(new MagickColor(0,0,0),500,500);
        }
    }

    public static async Task<MagickImage> ConvertToImageAsync(this DiscordUser member, HttpClient httpClient)
    {
        var imageStream = await httpClient.GetStreamAsync(member.GetAvatarUrl(DSharpPlus.ImageFormat.Auto));
        try
        {
            return new MagickImage(imageStream);
        }
        catch (System.Exception e)
        {
            Log.Error(e, "Image conversion error:");
            return new MagickImage(new MagickColor(0,0,0),500,500);
        }
    }
}