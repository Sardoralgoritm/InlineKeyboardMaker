using Telegram.Bot.Types;

namespace InlineKeyboardBot.Handlers.Interfaces;

public interface IUpdateHandler
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
}

public interface ICommandHandler
{
    Task HandleCommandAsync(Message message, string command, CancellationToken cancellationToken);
    bool CanHandle(string command);
}

public interface ICallbackHandler
{
    Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken);
    bool CanHandle(string callbackData);
}

public interface IMessageHandler
{
    Task HandleMessageAsync(Message message, CancellationToken cancellationToken);
    bool CanHandle(Message message);
}