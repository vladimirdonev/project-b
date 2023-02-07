using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace ProjectB.Handlers
{
    public class TelegramUpdateHandler : ITelegramUpdateHandler
    {
        private readonly IStateFactory _statefactory;

        public TelegramUpdateHandler(IStateFactory statefactory)
        {
            _statefactory = statefactory;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message == null && update.CallbackQuery == null)
            {
                return;
            }

            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Text.ToString().ToLower() == "/start")
                {
                  await HandleCommunication(botClient, update, _statefactory.GetState(State.MainState));
                }
                else
                {
                  await HandleCommunication(botClient, update, _statefactory.GetState(State.CityTypedFromUserState));
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var currentState = update.CallbackQuery.Data.Split(" ").Count() == 1 ? (State)Enum.Parse(typeof(State), update.CallbackQuery.Data) : 
                    (State)Enum.Parse(typeof(State), update.CallbackQuery.Data.Split(" ")[1]);
                await HandleCommunication(botClient, update, _statefactory.GetState(currentState));
            }
            
        }

        private async Task HandleCommunication(ITelegramBotClient botClient, Update update, IState state)
        {

            try
            {
                _ = update.Type switch
                {
                    UpdateType.Message => await state.BotOnMessageReceived(botClient, update.Message),
                    UpdateType.CallbackQuery => await state.BotOnCallBackQueryReceived(botClient, update.CallbackQuery),
                    _ => await UnknownUpdateHandlerAsync(botClient, update)
                };
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(await GetChatId(update), ex.Message);
                await RepeatState(ex, botClient, update);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            return Task.CompletedTask;
        }

        private async Task<State> UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong! Please try again");

            return State.MainState;
        }

        private async Task<long> GetChatId(Update update)
        {
            return update.Message != null ? update.Message.Chat.Id : update.CallbackQuery.Message.Chat.Id;
        }

        private async Task RepeatState(Exception ex, ITelegramBotClient botClient, Update update)
        {
            if (ex.StackTrace.Contains("GetDestinationIdAsync"))
            {
                await HandleCommunication(botClient, update, _statefactory.GetState(State.CitySelectState));
            }
            else if (ex.StackTrace.Contains("HotelInfoState"))
            {
              await HandleCommunication(botClient, update, _statefactory.GetState(State.CityTypedFromUserState));
            }

        }
    }
}
