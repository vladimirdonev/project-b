namespace ProjectB.States;

public class MainState : IState
{
    public async Task<State> BotOnCallBackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
       await BotSendMessage(botClient, callbackQuery.Message.Chat.Id);
        return State.MainState;
    }

    public async Task<State> BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    {
        await BotSendMessage(botClient, message.Chat.Id);
        return State.MainState;
    }

    public async Task BotSendMessage(ITelegramBotClient botClient, long chatId)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("Help",State.HelpState.ToString()),
                InlineKeyboardButton.WithCallbackData("Hotels", State.CitySelectState.ToString()),
            },
        });
        var message = new Message();
        message.Text = "Welcome Please choose from buttons below";
        await botClient.SendTextMessageAsync(chatId, message.Text, replyMarkup: inlineKeyboard);
    }
}
