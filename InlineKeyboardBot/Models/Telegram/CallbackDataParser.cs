namespace InlineKeyboardBot.Models.Telegram;

public class CallbackDataParser
{
    public string Command { get; private set; } = default!;
    public List<string> Parameters { get; private set; } = new();

    public static CallbackDataParser Parse(string callbackData)
    {
        var parts = callbackData.Split('_');

        if (parts.Length >= 3 && parts[0] == "select" && parts[1] == "channel")
        {
            return new CallbackDataParser
            {
                Command = "select_channel",
                Parameters = parts.Skip(2).ToList()  // ["guid-here"]
            };
        }

        return new CallbackDataParser
        {
            Command = parts[0],
            Parameters = parts.Skip(1).ToList()
        };
    }

    public string GetParameter(int index) =>
        index < Parameters.Count ? Parameters[index] : string.Empty;

    public Guid GetGuidParameter(int index) =>
        Guid.TryParse(GetParameter(index), out var guid) ? guid : Guid.Empty;

    public int GetIntParameter(int index) =>
        int.TryParse(GetParameter(index), out var value) ? value : 0;

    public bool HasParameter(int index) => index < Parameters.Count;
}