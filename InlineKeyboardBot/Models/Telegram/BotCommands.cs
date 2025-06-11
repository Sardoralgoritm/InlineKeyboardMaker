namespace InlineKeyboardBot.Models.Telegram;

public static class BotCommands
{
    // Main commands
    public const string Start = "/start";
    public const string Help = "/help";
    public const string Cancel = "/cancel";
    public const string Done = "/done";

    // Channel commands
    public const string Register = "/register";
    public const string AddChannel = "/addchannel";
    public const string MyChannels = "/mychannels";

    // Post commands
    public const string NewPost = "/newpost";
    public const string MyPosts = "/myposts";
    public const string Stats = "/stats";

    // Settings
    public const string Settings = "/settings";
    public const string Language = "/language";
}