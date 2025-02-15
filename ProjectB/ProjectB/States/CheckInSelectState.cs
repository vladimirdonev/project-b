﻿namespace ProjectB.States;

public class CheckInSelectState : IState
{
    private readonly ICosmosDbService<UserInformation> _cosmosDbService;

    public CheckInSelectState(ICosmosDbService<UserInformation> cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    public async Task<State> BotOnCallBackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var userInformation = new UserInformation();
        userInformation.Id = callbackQuery.Message.Chat.Id.ToString();
        userInformation.CheckInDate = callbackQuery.Data.Split(" ")[0];
        await _cosmosDbService.AddCheckInDateAsync(userInformation);
        await BotSendMessage(botClient, callbackQuery.Message.Chat.Id);
        return State.CheckInSelectState;
    }

    public async Task<State> BotOnMessageReceived(ITelegramBotClient botClient, Message message)
    => await Task.FromResult(State.CheckInSelectState);

    public async Task BotSendMessage(ITelegramBotClient botClient, long chatId)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
                // first row
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Show CheckOut Dates",State.CheckOutState.ToString()),
                }
        });

        await botClient.SendTextMessageAsync(chatId, "Select", replyMarkup: inlineKeyboard);
    }
}
