using DSharpPlus.Entities;
using ImageMagick;
using Microsoft.Extensions.Hosting;

namespace CDRB.Commands;

public class ImageCommands
{
    private readonly IHostEnvironment _hostEnviroment;
    private readonly MagickReadSettings _quoteNameSettings;
    private readonly MagickReadSettings _quoteTextSettings;
    private readonly MagickImage _wojakImage;

    public ImageCommands(IHostEnvironment hostEnvironment)
    {
        _hostEnviroment = hostEnvironment;
        var quoteNameFontPath = Path.Join(_hostEnviroment.ContentRootPath, "Resources", "Fonts", "LHandW.ttf");
        var quoteTextFontPath = Path.Join(_hostEnviroment.ContentRootPath, "Resources", "Fonts", "Gara.ttf");
        _quoteNameSettings = new()
        {
            Font = quoteNameFontPath,
            TextGravity = Gravity.Center,
            BackgroundColor = MagickColors.Transparent,
            Height = 35,
            Width = 420,
            FillColor = MagickColors.White
        };
        _quoteTextSettings = new()
        {
            Font = quoteTextFontPath,
            TextGravity = Gravity.Center,
            BackgroundColor = MagickColors.Transparent,
            Height = 245,
            Width = 485,
            FillColor = MagickColors.White,
        };
        _wojakImage = new MagickImage("Resources/Images/wojacktemplate.png");
    }

    private DiscordMessageBuilder prepareToSend(MagickImage image, string message, DiscordMember caller)
    {
        var imageStream = new MemoryStream();
        image.Write(imageStream, MagickFormat.Png);
        imageStream.Position = 0;
        var messageBuilder = new DiscordMessageBuilder()
            .AddFile("output.png", imageStream);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle(message)
            .WithAuthor(caller.DisplayName, null, caller.GetGuildAvatarUrl(DSharpPlus.ImageFormat.Auto))
            .WithImageUrl("attachment://output.png");
        messageBuilder.AddEmbed(embedBuilder);
        return messageBuilder;
    }

    private DiscordMessageBuilder prepareToSend(MagickImageCollection animation, string message, DiscordMember caller)
    {
        animation[0].AnimationIterations = 0;
        var imageStream = new MemoryStream();
        animation.Write(imageStream, MagickFormat.Gif);
        imageStream.Position = 0;
        var messageBuilder = new DiscordMessageBuilder()
            .AddFile("output.gif", imageStream);
        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle(message)
            .WithAuthor(caller.DisplayName, null, caller.GetGuildAvatarUrl(DSharpPlus.ImageFormat.Auto))
            .WithImageUrl("attachment://output.gif");
        messageBuilder.AddEmbed(embedBuilder);
        return messageBuilder;
    }

    public DiscordMessageBuilder WojakPhoto(MagickImage victimPhoto, DiscordMember caller, string name = "")
    {
        IMagickImage<byte> wojakMask = _wojakImage.Clone();
        MagickImage imageCanvas = new(MagickColors.White, _wojakImage.Width, _wojakImage.Height);
        victimPhoto.Resize(350, 350);
        imageCanvas.Composite(victimPhoto, 120, 60, CompositeOperator.Over);
        imageCanvas.Composite(wojakMask, 0, 0, CompositeOperator.Over);
        return prepareToSend(imageCanvas, $"{name} has been drawn as a crying wojak. They have lost the argument", caller);
    }

    public DiscordMessageBuilder ImpactPhoto(MagickImage background, string topText, string bottomText, DiscordMember caller)
    {
        double strokeWidth = ((background.Width*0.005)+(background.Height*0.005))/2;
        MagickReadSettings impactTextSettings = new()
        {
            Font = Path.Join(_hostEnviroment.ContentRootPath, "Resources", "Fonts", "Impact.ttf"),
            TextGravity = Gravity.Center,
            BackgroundColor = MagickColors.Transparent,
            Height = background.Height / 5,
            Width = background.Width,
            StrokeColor = MagickColors.Black,
            StrokeWidth = strokeWidth,
            FillColor = MagickColors.White
        };
        MagickImage topCaption = new($"caption:{topText}", impactTextSettings);
        MagickImage bottomCaption = new($"caption:{bottomText}", impactTextSettings);

        background.Composite(topCaption, 0, 0, CompositeOperator.Over);
        background.Composite(bottomCaption, 0, background.Height - (background.Height / 5), CompositeOperator.Over);
        return prepareToSend(background, "Output", caller);
    }

    public DiscordMessageBuilder InflatePhoto(MagickImage victimPhoto, DiscordMember caller, string name = "")
    {
        MagickImageCollection bigAndRoundAnimation = new();
        victimPhoto.Resize(250, 250);
        for (float i = 0; i <= 0.8; i += 0.05f)
        {
            IMagickImage<byte> tempImage = victimPhoto.Clone();
            tempImage.Distort(DistortMethod.Barrel, i, 0.0, 0.0);
            tempImage.AnimationDelay = 10;
            bigAndRoundAnimation.Add(tempImage);
        }
        bigAndRoundAnimation.Last().AnimationDelay = 100;
        return prepareToSend(bigAndRoundAnimation, $"{name} has been inflated big and round.", caller);
    }
}