using InlineKeyboardBot.Models.Dto.Enums;
using InlineKeyboardBot.Models.Dto;
using Telegram.Bot.Types.ReplyMarkups;

namespace InlineKeyboardBot.Models.Telegram;

public static class InlineKeyboardHelper
{
    public static InlineKeyboardMarkup CreateMainMenu()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🆕 Yangi Post", CallbackCommands.NewPost),
                InlineKeyboardButton.WithCallbackData("📋 Mening Postlarim", CallbackCommands.MyPosts)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📺 Mening Kanallarim", CallbackCommands.MyChannels),
                InlineKeyboardButton.WithCallbackData("📊 Statistika", CallbackCommands.Settings)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⚙️ Sozlamalar", CallbackCommands.Settings)
            }
        });
    }

    public static InlineKeyboardMarkup CreateButtonLayoutMenu()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("1️⃣ Har biri alohida", CallbackCommands.LayoutSingle)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("2️⃣ Ikkitadan bir qatorda", CallbackCommands.LayoutDouble)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("3️⃣ Uchta bir qatorda", CallbackCommands.LayoutTriple)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Barchasini bir qatorda", CallbackCommands.LayoutOneRow)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✏️ Maxsus tartib", CallbackCommands.LayoutCustom)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔙 Orqaga", CallbackCommands.BackToMenu)
            }
        });
    }

    public static InlineKeyboardMarkup CreatePostPreviewActions()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Tasdiqlash", CallbackCommands.ConfirmPost),
                InlineKeyboardButton.WithCallbackData("✏️ Tahrirlash", CallbackCommands.EditPost)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🗑 Bekor qilish", CallbackCommands.CancelPost)
            }
        });
    }

    public static InlineKeyboardMarkup CreateChannelList(List<ChannelDto> channels)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var channel in channels)
        {
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"📺 {channel.DisplayName}",
                    CallbackCommands.SelectChannel + channel.Id)
            });
        }

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("➕ Yangi kanal qo'shish", CallbackCommands.AddChannel)
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Orqaga", CallbackCommands.BackToMenu)
        });

        return new InlineKeyboardMarkup(buttons);
    }

    public static InlineKeyboardMarkup CreatePagination(int currentPage, int totalPages, string baseCommand)
    {
        var buttons = new List<InlineKeyboardButton>();

        if (currentPage > 1)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData("⬅️",
                CallbackCommands.PrevPage + (currentPage - 1)));
        }

        buttons.Add(InlineKeyboardButton.WithCallbackData($"{currentPage}/{totalPages}",
            CallbackCommands.GoToPage + currentPage));

        if (currentPage < totalPages)
        {
            buttons.Add(InlineKeyboardButton.WithCallbackData("➡️",
                CallbackCommands.NextPage + (currentPage + 1)));
        }

        return new InlineKeyboardMarkup(new[] { buttons.ToArray() });
    }

    public static InlineKeyboardMarkup CreateFromButtons(List<InlineButtonDto> buttons, ButtonLayoutType layout)
    {
        if (!buttons.Any())
            return new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton[]>());

        return layout switch
        {
            ButtonLayoutType.SingleColumn => CreateSingleColumnLayout(buttons),
            ButtonLayoutType.TwoColumns => CreateMultiColumnLayout(buttons, 2),
            ButtonLayoutType.ThreeColumns => CreateMultiColumnLayout(buttons, 3),
            ButtonLayoutType.AllInOneRow => CreateSingleRowLayout(buttons),
            ButtonLayoutType.Custom => CreateCustomLayout(buttons),
            _ => CreateSingleColumnLayout(buttons)
        };
    }

    private static InlineKeyboardMarkup CreateSingleColumnLayout(List<InlineButtonDto> buttons)
    {
        var keyboard = buttons.Select(button => new[]
        {
            InlineKeyboardButton.WithUrl(button.Text, button.Url)
        }).ToArray();

        return new InlineKeyboardMarkup(keyboard);
    }

    private static InlineKeyboardMarkup CreateMultiColumnLayout(List<InlineButtonDto> buttons, int columns)
    {
        var keyboard = new List<InlineKeyboardButton[]>();

        for (int i = 0; i < buttons.Count; i += columns)
        {
            var row = buttons.Skip(i).Take(columns)
                .Select(button => InlineKeyboardButton.WithUrl(button.Text, button.Url))
                .ToArray();
            keyboard.Add(row);
        }

        return new InlineKeyboardMarkup(keyboard);
    }

    private static InlineKeyboardMarkup CreateSingleRowLayout(List<InlineButtonDto> buttons)
    {
        var row = buttons.Select(button =>
            InlineKeyboardButton.WithUrl(button.Text, button.Url)).ToArray();

        return new InlineKeyboardMarkup(new[] { row });
    }

    private static InlineKeyboardMarkup CreateCustomLayout(List<InlineButtonDto> buttons)
    {
        var groupedButtons = buttons.GroupBy(b => b.Row).OrderBy(g => g.Key);
        var keyboard = new List<InlineKeyboardButton[]>();

        foreach (var row in groupedButtons)
        {
            var rowButtons = row.OrderBy(b => b.Column)
                .Select(button => InlineKeyboardButton.WithUrl(button.Text, button.Url))
                .ToArray();
            keyboard.Add(rowButtons);
        }

        return new InlineKeyboardMarkup(keyboard);
    }
}