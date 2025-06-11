using System.Text;

namespace InlineKeyboardBot.Models.Telegram;

public class CallbackDataBuilder
{
    private readonly StringBuilder _builder = new();

    public CallbackDataBuilder(string command)
    {
        _builder.Append(command);
    }

    public CallbackDataBuilder WithParameter(string parameter)
    {
        _builder.Append($"_{parameter}");
        return this;
    }

    public CallbackDataBuilder WithParameter(Guid parameter)
    {
        _builder.Append($"_{parameter}");
        return this;
    }

    public CallbackDataBuilder WithParameter(int parameter)
    {
        _builder.Append($"_{parameter}");
        return this;
    }

    public string Build() => _builder.ToString();

    public static CallbackDataBuilder Create(string command) => new(command);
}
