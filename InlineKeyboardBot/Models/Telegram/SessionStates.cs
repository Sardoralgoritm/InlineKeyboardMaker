namespace InlineKeyboardBot.Models.Telegram;

public static class SessionStates
{
    // Post creation workflow
    public const string CreatingPost = "creating_post";
    public const string AddingButtons = "adding_buttons";
    public const string ConfiguringButtons = "configuring_buttons";
    public const string SchedulingPost = "scheduling_post";

    // Channel management
    public const string ClaimingChannel = "claiming_channel";
    public const string RegisteringChannel = "registering_channel";

    // User input states
    public const string WaitingForInput = "waiting_for_input";
    public const string WaitingForChannelName = "waiting_for_channel_name";
    public const string WaitingForPostText = "waiting_for_post_text";

    // Settings workflow
    public const string UpdatingSettings = "updating_settings";
    public const string ChangingLanguage = "changing_language";
}
