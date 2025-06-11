namespace InlineKeyboardBot.Models.Telegram;

public class BotCommandInfo
{
    public string Command { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsAdminOnly { get; set; }
    public bool IsChannelOnly { get; set; }

    public static List<BotCommandInfo> GetAllCommands()
    {
        return new List<BotCommandInfo>
        {
            new() { Command = BotCommands.Start, Description = "Botni boshlash" },
            new() { Command = BotCommands.Help, Description = "Yordam ma'lumotlari" },
            new() { Command = BotCommands.NewPost, Description = "Yangi post yaratish" },
            new() { Command = BotCommands.MyPosts, Description = "Mening postlarim" },
            new() { Command = BotCommands.MyChannels, Description = "Mening kanallarim" },
            new() { Command = BotCommands.Stats, Description = "Statistika" },
            new() { Command = BotCommands.Settings, Description = "Sozlamalar" },
            new() { Command = BotCommands.Register, Description = "Kanalni ro'yxatga olish", IsChannelOnly = true },
            new() { Command = BotCommands.Cancel, Description = "Joriy amalni bekor qilish" }
        };
    }
}