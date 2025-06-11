using InlineKeyboardBot.Handlers.Interfaces;
using InlineKeyboardBot.Models.Telegram;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
namespace InlineKeyboardBot.Handlers;

public class UpdateDistributor : IUpdateHandler
{
    private readonly CommandHandler _commandHandler;
    private readonly CallbackHandler _callbackHandler;
    private readonly MessageHandler _messageHandler;
    private readonly ILogger<UpdateDistributor> _logger;

    public UpdateDistributor(
        CommandHandler commandHandler,
        CallbackHandler callbackHandler,
        MessageHandler messageHandler,
        ILogger<UpdateDistributor> logger)
    {
        _commandHandler = commandHandler;
        _callbackHandler = callbackHandler;
        _messageHandler = messageHandler;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing update {UpdateId} of type {UpdateType}",
                update.Id, update.Type);

            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageUpdate(update.Message!, cancellationToken),
                UpdateType.CallbackQuery => HandleCallbackUpdate(update.CallbackQuery!, cancellationToken),
                UpdateType.EditedMessage => HandleEditedMessageUpdate(update.EditedMessage!, cancellationToken),
                UpdateType.ChannelPost => HandleChannelPostUpdate(update.ChannelPost!, cancellationToken),
                _ => HandleUnsupportedUpdate(update)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update {UpdateId}", update.Id);
        }
    }

    private async Task HandleMessageUpdate(Message message, CancellationToken cancellationToken)
    {
        if (message.From?.IsBot == true)
        {
            _logger.LogDebug("Ignoring message from bot {BotId}", message.From.Id);
            return;
        }

        // Command yoki oddiy message ekanligini aniqlash
        if (message.Type == MessageType.Text && message.Text?.StartsWith("/") == true)
        {
            var command = message.Text.Split(' ')[0].ToLower();

            if (_commandHandler.CanHandle(command))
            {
                await _commandHandler.HandleCommandAsync(message, command, cancellationToken);
            }
            else
            {
                _logger.LogWarning("Unknown command: {Command} from user {UserId}",
                    command, message.From?.Id);

                // Unknown command ni message handler ga yuborish
                await _messageHandler.HandleMessageAsync(message, cancellationToken);
            }
        }
        else
        {
            // Oddiy message
            if (_messageHandler.CanHandle(message))
            {
                await _messageHandler.HandleMessageAsync(message, cancellationToken);
            }
        }
    }

    private async Task HandleCallbackUpdate(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.From.IsBot)
        {
            _logger.LogDebug("Ignoring callback from bot {BotId}", callbackQuery.From.Id);
            return;
        }

        var callbackData = callbackQuery.Data ?? string.Empty;

        if (_callbackHandler.CanHandle(callbackData))
        {
            await _callbackHandler.HandleCallbackAsync(callbackQuery, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Unknown callback data: {CallbackData} from user {UserId}",
                callbackData, callbackQuery.From.Id);
        }
    }

    private async Task HandleEditedMessageUpdate(Message editedMessage, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Edited message received from user {UserId}", editedMessage.From?.Id);

        // Edited message'larni oddiy message kabi handle qilish
        await HandleMessageUpdate(editedMessage, cancellationToken);
    }

    private async Task HandleChannelPostUpdate(Message channelPost, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Channel post received in chat {ChatId}", channelPost.Chat.Id);

        // Channel post'larni handle qilish (agar kerak bo'lsa)
        // Masalan, kanal registratsiyasi uchun /register command
        if (channelPost.Type == MessageType.Text &&
            channelPost.Text?.StartsWith("/") == true)
        {
            var command = channelPost.Text.Split(' ')[0].ToLower();

            if (command == BotCommands.Register)
            {
                await _commandHandler.HandleCommandAsync(channelPost, command, cancellationToken);
            }
        }
    }

    private Task HandleUnsupportedUpdate(Update update)
    {
        _logger.LogInformation("Unsupported update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}