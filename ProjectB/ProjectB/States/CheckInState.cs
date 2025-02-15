﻿namespace ProjectB.States;

public class CheckInState : IState
{

    public async Task<State> BotOnCallBackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        await BotSendMessage(botClient, callbackQuery.Message.Chat.Id);
        return State.CheckInState;
    }

    public async Task<State> BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    => await Task.FromResult(State.CheckInState);

    public async Task BotSendMessage(ITelegramBotClient botClient, long chatId)
    {
        var message = new Message();
        var choosedate = DateTime.UtcNow;
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData(choosedate.ToString("yyyy-MM-dd"), $"{choosedate.ToString("yyyy-MM-dd")} {State.CheckInSelectState}"),
                }
        });
        message.Text = "Choose checkin date";
        message.ReplyMarkup = inlineKeyboard;
        await botClient.SendTextMessageAsync(chatId, message.Text, replyMarkup: inlineKeyboard);
    }
}
