namespace InlineKeyboardBot.Models.Telegram;

public static class CallbackCommands
{
    // Main menu
    public const string NewPost = "new_post";
    public const string MyPosts = "my_posts";
    public const string MyChannels = "my_channels";
    public const string Settings = "settings";

    // Post creation
    public const string AddButtons = "add_buttons";
    public const string SkipButtons = "skip_buttons";
    public const string FinishButtons = "finish_buttons";
    public const string PreviewPost = "preview_post";
    public const string ConfirmPost = "confirm_post";
    public const string EditPost = "edit_post";
    public const string CancelPost = "cancel_post";

    // Button layout
    public const string LayoutSingle = "layout_single";
    public const string LayoutDouble = "layout_double";
    public const string LayoutTriple = "layout_triple";
    public const string LayoutOneRow = "layout_onerow";
    public const string LayoutCustom = "layout_custom";

    // Channel management
    public const string AddChannel = "add_channel";
    public const string SelectChannel = "select_channel";
    public const string RemoveChannel = "remove_channel";
    public const string ChannelStats = "channel_stats";

    // Post management
    public const string ViewPost = "view_post_";
    public const string EditPostContent = "edit_post_";
    public const string DeletePost = "delete_post_";
    public const string ResendPost = "resend_post_";
    public const string PostStats = "post_stats_";

    // Pagination
    public const string PrevPage = "prev_page_";
    public const string NextPage = "next_page_";
    public const string GoToPage = "goto_page_";

    // Confirmation
    public const string ConfirmYes = "confirm_yes_";
    public const string ConfirmNo = "confirm_no_";

    // Back navigation
    public const string BackToMenu = "back_menu";
    public const string BackToChannels = "back_channels";
    public const string BackToPosts = "back_posts";
}