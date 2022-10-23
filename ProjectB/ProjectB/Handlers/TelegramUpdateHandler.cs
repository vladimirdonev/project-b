using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

namespace ProjectB.Handlers
{
    public class TelegramUpdateHandler : ITelegramUpdateHandler
    {
        private IStateFactory _statefactory;

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

            var chatId = GetChatId(update);

            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Text.ToString().ToLower() == "/start")
                {
                   HandleCommunication(botClient, update, _statefactory.GetState(State.MainState));
                }
                else
                {
                    HandleCommunication(botClient, update, _statefactory.GetState(State.CityTypedFromUserState));
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var currentState = update.CallbackQuery.Data.Split(" ").Count() == 1 ? (State)Enum.Parse(typeof(State), update.CallbackQuery.Data) : 
                    (State)Enum.Parse(typeof(State), update.CallbackQuery.Data.Split(" ")[1]);
                HandleCommunication(botClient, update, _statefactory.GetState(currentState));
            }
            
        }

        private async void HandleCommunication(ITelegramBotClient botClient, Update update, IState state)
        {

            try
            {
                var handler = update.Type switch
                {
                    UpdateType.Message => await state.BotOnMessageReceived(botClient, update.Message),
                    UpdateType.CallbackQuery => await state.BotOnCallBackQueryReceived(botClient, update.CallbackQuery),
                    _ => await UnknownUpdateHandlerAsync(botClient, update)
                };
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(await GetChatId(update), ex.Message);
                RepeatState(ex, botClient, update);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        private async Task<State> UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Something went wrong! Please try again");

            return await Task.Run(() => State.MainState);
        }

        private async Task<long> GetChatId(Update update)
        {
            return update.Message != null ? update.Message.Chat.Id : update.CallbackQuery.Message.Chat.Id;
        }

        private async void RepeatState(Exception ex, ITelegramBotClient botClient, Update update)
        {
            if (ex.StackTrace.Contains("GetDestinationIdAsync"))
            {
                HandleCommunication(botClient, update, _statefactory.GetState(State.CitySelectState));
            }
            else if (ex.StackTrace.Contains("HotelInfoState"))
            {
               HandleCommunication(botClient, update, _statefactory.GetState(State.CityTypedFromUserState));
            }

        }
    }
}
