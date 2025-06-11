using InlineKeyboardBot.Handlers.Interfaces;
using InlineKeyboardBot.Models.Telegram;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace InlineKeyboardBot.Handlers;

public class CallbackHandler : ICallbackHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IChannelService _channelService;
    private readonly ILogger<CallbackHandler> _logger;

    public CallbackHandler(
        ITelegramBotClient botClient,
        IUserService userService,
        IChannelService channelService,
        ILogger<CallbackHandler> logger)
    {
        _botClient = botClient;
        _userService = userService;
        _channelService = channelService;
        _logger = logger;
    }

    public bool CanHandle(string callbackData)
    {
        if (string.IsNullOrEmpty(callbackData))
            return false;

        var parser = CallbackDataParser.Parse(callbackData);

        return parser.Command switch
        {
            CallbackCommands.NewPost => true,
            CallbackCommands.MyPosts => true,
            CallbackCommands.MyChannels => true,
            CallbackCommands.Settings => true,
            CallbackCommands.AddChannel => true,
            CallbackCommands.AddButtons => true,
            CallbackCommands.SkipButtons => true,
            CallbackCommands.FinishButtons => true,
            CallbackCommands.PreviewPost => true,
            CallbackCommands.ConfirmPost => true,
            CallbackCommands.EditPost => true,
            CallbackCommands.CancelPost => true,
            CallbackCommands.LayoutSingle => true,
            CallbackCommands.LayoutDouble => true,
            CallbackCommands.LayoutTriple => true,
            CallbackCommands.LayoutOneRow => true,
            CallbackCommands.LayoutCustom => true,
            CallbackCommands.BackToMenu => true,
            CallbackCommands.BackToChannels => true,
            CallbackCommands.BackToPosts => true,
            var cmd when cmd.StartsWith("select_channel_") => true,
            var cmd when cmd.StartsWith("remove_channel_") => true,
            var cmd when cmd.StartsWith("channel_stats_") => true,
            var cmd when cmd.StartsWith("view_post_") => true,
            var cmd when cmd.StartsWith("edit_post_") => true,
            var cmd when cmd.StartsWith("delete_post_") => true,
            var cmd when cmd.StartsWith("confirm_yes_") => true,
            var cmd when cmd.StartsWith("confirm_no_") => true,
            _ => false
        };
    }

    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling callback {CallbackData} from user {UserId}",
                callbackQuery.Data, callbackQuery.From.Id);

            // Answer callback query first
            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                cancellationToken: cancellationToken);

            var parser = CallbackDataParser.Parse(callbackQuery.Data!);

            var handler = parser.Command switch
            {
                CallbackCommands.NewPost => HandleNewPost(callbackQuery, cancellationToken),
                CallbackCommands.MyPosts => HandleMyPosts(callbackQuery, cancellationToken),
                CallbackCommands.MyChannels => HandleMyChannels(callbackQuery, cancellationToken),
                CallbackCommands.AddChannel => HandleAddChannel(callbackQuery, cancellationToken),
                CallbackCommands.AddButtons => HandleAddButtons(callbackQuery, cancellationToken),
                CallbackCommands.SkipButtons => HandleSkipButtons(callbackQuery, cancellationToken),
                CallbackCommands.FinishButtons => HandleFinishButtons(callbackQuery, cancellationToken),
                CallbackCommands.PreviewPost => HandlePreviewPost(callbackQuery, cancellationToken),
                CallbackCommands.ConfirmPost => HandleConfirmPost(callbackQuery, cancellationToken),
                CallbackCommands.EditPost => HandleEditPost(callbackQuery, cancellationToken),
                CallbackCommands.CancelPost => HandleCancelPost(callbackQuery, cancellationToken),
                CallbackCommands.LayoutSingle => HandleLayoutSelection(callbackQuery, "single", cancellationToken),
                CallbackCommands.LayoutDouble => HandleLayoutSelection(callbackQuery, "double", cancellationToken),
                CallbackCommands.LayoutTriple => HandleLayoutSelection(callbackQuery, "triple", cancellationToken),
                CallbackCommands.LayoutOneRow => HandleLayoutSelection(callbackQuery, "onerow", cancellationToken),
                CallbackCommands.LayoutCustom => HandleLayoutSelection(callbackQuery, "custom", cancellationToken),
                CallbackCommands.BackToMenu => HandleBackToMenu(callbackQuery, cancellationToken),
                CallbackCommands.BackToChannels => HandleBackToChannels(callbackQuery, cancellationToken),
                CallbackCommands.BackToPosts => HandleBackToPosts(callbackQuery, cancellationToken),
                var cmd when cmd.StartsWith("select_channel") => HandleSelectChannel(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("remove_channel") => HandleRemoveChannel(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("channel_stats") => HandleChannelStats(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("view_post") => HandleViewPost(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("edit_post") => HandleEditPostCallback(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("delete_post") => HandleDeletePost(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("confirm_yes") => HandleConfirmYes(callbackQuery, parser, cancellationToken),
                var cmd when cmd.StartsWith("confirm_no") => HandleConfirmNo(callbackQuery, parser, cancellationToken),
                _ => HandleUnknownCallback(callbackQuery, cancellationToken)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling callback {CallbackData}", callbackQuery.Data);

            await _botClient.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                "❌ Xatolik yuz berdi!",
                showAlert: true,
                cancellationToken: cancellationToken);
        }
    }

    private async Task HandleNewPost(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: """
                🆕 **Yangi post yaratish**
                
                Post matnini yuboring.
                
                📝 **Maslahatlar:**
                • Emoji ishlatishingiz mumkin
                • Maksimal 4096 belgi
                • Markdown formatlash qo'llab-quvvatlanadi
                
                Bekor qilish uchun /cancel buyrug'ini yuboring.
                """,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleAddChannel(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: """
                ➕ **Yangi kanal qo'shish**
                
                **Qadamlar:**
                1. Botni kanalingizga admin qiling
                2. Quyidagi huquqlarni bering:
                   ✅ Send Messages
                   ✅ Edit Messages  
                   ✅ Delete Messages
                3. Kanal ichida /register buyrug'ini yuboring
                
                **Eslatma:** Bot faqat siz admin bo'lgan kanallarga post yuborishi mumkin.
                """,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: InlineKeyboardHelper.CreateMainMenu(),
            cancellationToken: cancellationToken);
    }

    private async Task HandleAddButtons(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: """
                🔘 **Tugma qo'shish**
                
                Tugma nomi va linkini quyidagi formatda yuboring:
                
                **Format:** `Tugma nomi | https://example.com`
                
                **Misol:** `Sotib olish | https://myshop.com`
                
                📌 Tugmalarni tugatish uchun /done buyrug'ini yuboring.
                """,
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            cancellationToken: cancellationToken);
    }

    private async Task HandleSkipButtons(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // Tugmasiz post yaratish
        var channels = await _channelService.GetUserChannelsAsync(callbackQuery.From.Id);

        if (!channels.Any())
        {
            await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "❌ Avval kanal qo'shishingiz kerak!\n\n/mychannels buyrug'i bilan kanallar qo'shing.",
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = InlineKeyboardHelper.CreateChannelList(channels.ToList());

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "📺 **Qaysi kanalga yuboramiz?**\n\nKanalni tanlang:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleFinishButtons(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // Tugmalar tugadi, layout tanlash
        var keyboard = InlineKeyboardHelper.CreateButtonLayoutMenu();

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: """
                📐 **Tugmalar tartibini tanlang:**
                
                Tugmalar qanday joylashsin?
                """,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandlePreviewPost(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // Post preview ko'rsatish
        // Bu qismni PostService bilan implement qilamiz

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "👀 Post preview qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleConfirmPost(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // Post ni tasdiqlash va yuborish
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "✅ Post yuborish qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleEditPost(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "✏️ Post tahrirlash qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleCancelPost(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var keyboard = InlineKeyboardHelper.CreateMainMenu();

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "❌ Post yaratish bekor qilindi.\n\nQuyidagi tugmalardan birini tanlang:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleLayoutSelection(CallbackQuery callbackQuery, string layoutType, CancellationToken cancellationToken)
    {
        var layoutName = layoutType switch
        {
            "single" => "Har biri alohida qatorda",
            "double" => "Ikkitadan bir qatorda",
            "triple" => "Uchta bir qatorda",
            "onerow" => "Barchasini bir qatorda",
            "custom" => "Maxsus tartib",
            _ => "Noma'lum"
        };

        // Kanal tanlash
        var channels = await _channelService.GetUserChannelsAsync(callbackQuery.From.Id);

        if (!channels.Any())
        {
            await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "❌ Avval kanal qo'shishingiz kerak!\n\n/mychannels buyrug'i bilan kanallar qo'shing.",
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = InlineKeyboardHelper.CreateChannelList(channels.ToList());

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"✅ Tugmalar tartibi tanlandi: **{layoutName}**\n\n📺 Qaysi kanalga yuboramiz?",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleBackToMenu(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var keyboard = InlineKeyboardHelper.CreateMainMenu();

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "🏠 **Asosiy menyu**\n\nQuyidagi tugmalardan birini tanlang:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleBackToChannels(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await HandleMyChannels(callbackQuery, cancellationToken);
    }

    private async Task HandleBackToPosts(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await HandleMyPosts(callbackQuery, cancellationToken);
    }

    private async Task HandleSelectChannel(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        if (!parser.HasParameter(0))
        {
            await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: "❌ Kanal ID topilmadi!",
                cancellationToken: cancellationToken);
            return;
        }

        var channelId = parser.GetGuidParameter(0);

        // Kanal ma'lumotlarini olish va post yuborish
        // Bu qismni ChannelService va PostService bilan implement qilamiz

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: $"📺 Kanal tanlandi! Post yuborish qismi hali ishlab chiqilmoqda...\n\nKanal ID: {channelId}",
            cancellationToken: cancellationToken);
    }

    private async Task HandleRemoveChannel(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var channelId = parser.GetGuidParameter(0);

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "🗑 Kanal o'chirish qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleChannelStats(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var channelId = parser.GetGuidParameter(0);

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "📊 Kanal statistikasi qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleViewPost(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var postId = parser.GetGuidParameter(0);

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "👀 Post ko'rish qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleEditPostCallback(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var postId = parser.GetGuidParameter(0);

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "✏️ Post tahrirlash qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleDeletePost(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var postId = parser.GetGuidParameter(0);

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "🗑 Post o'chirish qismi hali ishlab chiqilmoqda...",
            cancellationToken: cancellationToken);
    }

    private async Task HandleConfirmYes(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "✅ Tasdiqlandi!",
            cancellationToken: cancellationToken);
    }

    private async Task HandleConfirmNo(CallbackQuery callbackQuery, CallbackDataParser parser, CancellationToken cancellationToken)
    {
        var keyboard = InlineKeyboardHelper.CreateMainMenu();

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "❌ Bekor qilindi.\n\nAsosiy menyuga qaytish:",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }

    private async Task HandleUnknownCallback(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Unknown callback data: {CallbackData}", callbackQuery.Data);

        await _botClient.AnswerCallbackQueryAsync(
            callbackQuery.Id,
            "❓ Noma'lum buyruq!",
            showAlert: true,
            cancellationToken: cancellationToken);
    }
}

    private async Task HandleMyPosts(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "📋 Bu qism hali ishlab chiqilmoqda...\n\nTez orada tayyor bo'ladi! 🚀",
            cancellationToken: cancellationToken);
    }

    private async Task HandleMyChannels(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var channels = await _channelService.GetUserChannelsAsync(callbackQuery.From.Id);

        if (!channels.Any())
        {
            await _botClient.EditMessageTextAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: """
                    📺 Sizda hali kanallar yo'q.
                    
                    **Kanal qo'shish uchun:**
                    1. Botni kanalingizga admin qiling
                    2. Kanal ichida /register buyrug'ini yuboring
                    """,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: cancellationToken);
            return;
        }

        var keyboard = InlineKeyboardHelper.CreateChannelList(channels.ToList());

        await _botClient.EditMessageTextAsync(
            chatId: callbackQuery.Message!.Chat.Id,
            messageId: callbackQuery.Message.MessageId,
            text: "📺 **Sizning kanallaringiz:**\n\nBoshqarish uchun kanalni tanlang:",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}