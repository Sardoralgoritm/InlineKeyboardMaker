using InlineKeyboardBot.Handlers.Interfaces;
using InlineKeyboardBot.Models.Telegram;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace InlineKeyboardBot.Handlers;

public class CommandHandler : ICommandHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IChannelService _channelService;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        ITelegramBotClient botClient,
        IUserService userService,
        IChannelService channelService,
        ILogger<CommandHandler> logger)
    {
        _botClient = botClient;
        _userService = userService;
        _channelService = channelService;
        _logger = logger;
    }

    public bool CanHandle(string command)
    {
        return command switch
        {
            BotCommands.Start => true,
            BotCommands.Help => true,
            BotCommands.Cancel => true,
            BotCommands.Register => true,
            BotCommands.MyChannels => true,
            BotCommands.NewPost => true,
            BotCommands.MyPosts => true,
            BotCommands.Stats => true,
            BotCommands.Settings => true,
            _ => false
        };
    }

    public async Task HandleCommandAsync(Message message, string command, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling command {Command} from user {UserId}",
                command, message.From?.Id);

            // User ma'lumotlarini olish/yaratish
            var user = await _userService.GetOrCreateUserAsync(
                message.From!.Id,
                message.From.Username ?? "",
                message.From.FirstName,
                message.From.LastName);

            var handler = command switch
            {
                BotCommands.Start => HandleStartCommand(message, cancellationToken),
                BotCommands.Help => HandleHelpCommand(message, cancellationToken),
                BotCommands.Cancel => HandleCancelCommand(message, cancellationToken),
                BotCommands.Register => HandleRegisterCommand(message, cancellationToken),
                BotCommands.MyChannels => HandleMyChannelsCommand(message, cancellationToken),
                BotCommands.NewPost => HandleNewPostCommand(message, cancellationToken),
                BotCommands.MyPosts => HandleMyPostsCommand(message, cancellationToken),
                BotCommands.Stats => HandleStatsCommand(message, cancellationToken),
                BotCommands.Settings => HandleSettingsCommand(message, cancellationToken),
                _ => HandleUnknownCommand(message, command, cancellationToken)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command {Command}", command);
            await SendErrorMessage(message.Chat.Id, "Buyruqni bajarishda xatolik yuz berdi.");
        }
    }

    private async Task HandleStartCommand(Message message, CancellationToken cancellationToken)
    {
        var welcomeText = $"""
            👋 Assalomu alaykum, {message.From!.FirstName}!
            
            Men **Inline Keyboard Bot**man. Men sizga kanallaringizga chiroyli tugmalar bilan post yuklashda yordam beraman.
            
            🎯 **Nimalar qila olaman:**
            • Inline tugmalar bilan post yaratish
            • Turli xil tugma tartibini sozlash  
            • Bir nechta kanalga post yuborish
            • Post statistikasini kuzatish
            
            📚 **Boshlash uchun:**
            1. Kanalingizni qo'shing
            2. Yangi post yarating
            3. Tugmalarni qo'shing
            4. Kanalingizga yuboring!
            
            Boshlash uchun quyidagi tugmalardan birini tanlang:
            """;

        var keyboard = InlineKeyboardHelper.CreateMainMenu();

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: welcomeText,
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleHelpCommand(Message message, CancellationToken cancellationToken)
    {
        var helpText = """
            📖 **Yordam ma'lumotlari**
            
            **🤖 Asosiy buyruqlar:**
            /start - Botni ishga tushirish
            /help - Yordam ma'lumotlari
            /newpost - Yangi post yaratish
            /mychannels - Kanallarim
            /myposts - Postlarim
            /stats - Statistika
            /settings - Sozlamalar
            /cancel - Joriy amalni bekor qilish
            
            **📺 Kanal qo'shish:**
            1. Botni kanalingizga admin qiling
            2. Kanal ichida /register buyrug'ini yuboring
            3. Bot kanal ma'lumotlarini saqlaydi
            
            **📝 Post yaratish:**
            1. "Yangi Post" tugmasini bosing
            2. Post matnini yozing
            3. Tugmalar qo'shing (ixtiyoriy)
            4. Tugma tartibini tanlang
            5. Kanalingizni tanlang va yuboring
            
            **🆘 Yordam kerakmi?**
            Agar muammoga duch kelsangiz, /cancel buyrug'ini ishlatib joriy amalni bekor qiling va qaytadan urinib ko'ring.
            """;

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helpText,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleCancelCommand(Message message, CancellationToken cancellationToken)
    {
        // User session'ini tozalash
        // Keyinroq UserSession service orqali implement qilamiz

        var keyboard = InlineKeyboardHelper.CreateMainMenu();

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "❌ Joriy amal bekor qilindi.\n\nQuyidagi tugmalardan birini tanlang:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleRegisterCommand(Message message, CancellationToken cancellationToken)
    {
        // Faqat kanal ichida ishlaydi
        if (message.Chat.Type != ChatType.Channel)
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Bu buyruq faqat kanal ichida ishlatiladi!",
                cancellationToken: cancellationToken);
            return;
        }

        try
        {
            var success = await _channelService.RegisterChannelAsync(message);

            if (success)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "✅ Kanal muvaffaqiyatli ro'yxatga qo'shildi!\n\nEndi siz bot orqali bu kanalga post yuborishingiz mumkin.",
                    cancellationToken: cancellationToken);
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "❌ Kanalni ro'yxatga qo'shishda xatolik yuz berdi.",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering channel {ChannelId}", message.Chat.Id);
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "❌ Kanalni ro'yxatga qo'shishda xatolik yuz berdi.",
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleMyChannelsCommand(Message message, CancellationToken cancellationToken)
    {
        var channels = await _channelService.GetUserChannelsAsync(message.From!.Id);

        if (!channels.Any())
        {
            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: """
                    📺 Sizda hali kanallar yo'q.
                    
                    **Kanal qo'shish uchun:**
                    1. Botni kanalingizga admin qiling
                    2. Kanal ichida /register buyrug'ini yuboring
                    
                    Batafsil ma'lumot uchun /help buyrug'ini yuboring.
                    """,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = InlineKeyboardHelper.CreateChannelList(channels.ToList());

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "📺 **Sizning kanallaringiz:**\n\nBoshqarish uchun kanalni tanlang:",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleNewPostCommand(Message message, CancellationToken cancellationToken)
    {
        // Post yaratish jarayonini boshlash
        // Bu qismni keyinroq PostService bilan implement qilamiz

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: """
                🆕 **Yangi post yaratish**
                
                Post matnini yuboring. 
                
                📝 **Maslahatlar:**
                • Emoji ishlatishingiz mumkin
                • Maksimal 4096 belgi
                • Markdown formatlash qo'llab-quvvatlanadi
                
                Bekor qilish uchun /cancel buyrug'ini yuboring.
                """,
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleMyPostsCommand(Message message, CancellationToken cancellationToken)
    {
        // Foydalanuvchi postlarini ko'rsatish
        // Bu qismni keyinroq PostService bilan implement qilamiz

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "📋 Bu qism hali ishlab chiqilmoqda...\n\nTez orada tayyor bo'ladi! 🚀",
            cancellationToken: cancellationToken);
    }

    private async Task HandleStatsCommand(Message message, CancellationToken cancellationToken)
    {
        // Statistika ko'rsatish
        // Bu qismni keyinroq StatisticsService bilan implement qilamiz

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "📊 Statistika qismi hali ishlab chiqilmoqda...\n\nTez orada tayyor bo'ladi! 📈",
            cancellationToken: cancellationToken);
    }

    private async Task HandleSettingsCommand(Message message, CancellationToken cancellationToken)
    {
        // Sozlamalar
        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "⚙️ Sozlamalar qismi hali ishlab chiqilmoqda...\n\nTez orada tayyor bo'ladi! 🔧",
            cancellationToken: cancellationToken);
    }

    private async Task HandleUnknownCommand(Message message, string command, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Unknown command {Command} from user {UserId}", command, message.From?.Id);

        await _botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"❓ Noma'lum buyruq: {command}\n\nMavjud buyruqlar ro'yxati uchun /help buyrug'ini yuboring.",
            cancellationToken: cancellationToken);
    }

    private async Task SendErrorMessage(long chatId, string errorMessage)
    {
        try
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"❌ {errorMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error message to chat {ChatId}", chatId);
        }
    }
}