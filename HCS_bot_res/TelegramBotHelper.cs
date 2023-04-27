using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;


namespace HCS_bot_test
{
    internal class TelegramBotHelper
    {
        public string _token { get; set; }

        private const string TEXT_CONSULT = "Получить консультацию";
        private const string TEXT_REQ = "Заявка";

        private const string TEXT_MY_REQ= "Мои заявки";
        private const string TEXT_LEAVE_REQ = "Сделать заявку";

        private const string TEXT_BACK = "Назад";


        UserRequest userRequets;

        Telegram.Bot.TelegramBotClient _client;
        private Dictionary<long, UserState> _clientState = new Dictionary<long, UserState>();

        private List<long> admins_id = new List<long> { 1610733398, 821204845 };

        public TelegramBotHelper(string token)
        {
            _token = token;
        }

        internal void GetUpdate()
        {
            _client = new Telegram.Bot.TelegramBotClient(_token);
            var me = _client.GetMeAsync().Result;
            if (me != null && !string.IsNullOrEmpty(me.Username))
            {
                int offset = 0;
                while (true)
                {
                    try
                    {

                        var updates = _client.GetUpdatesAsync(offset).Result;
                        if (updates != null && updates.Count() > 0)
                        {
                            foreach (var update in updates)
                            {
                                processUpdate(update);
                                offset = update.Id + 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        private void processUpdate(Update update)
        {
            switch (update.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                    var text = update.Message.Text;
                    var state = _clientState.ContainsKey(update.Message.Chat.Id) ? _clientState[update.Message.Chat.Id] : null;
                    if (state != null)
                    {
                        switch (state.State)
                        {
                            //Registering user
                            case State.RegisterUser:
                                _clientState[update.Message.Chat.Id] = new UserState { State = State.RegisterUserRepeat};
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Ваш номер телефона:\n{text}");
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Все верно?", replyMarkup: GetYesNoButtons());
                                break;
                            case State.RegisterUserRepeat:
                                if(text == "Да")
                                {
                                    RegisterUser(text);
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Регистрация прошла успешно!", replyMarkup: GetButtons());
                                    _clientState[update.Message.Chat.Id] = null;
                                }
                                else if(text == "Нет")
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Повторите попытку.");
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона в формате 79*********");
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.RegisterUser };
                                }
                                else
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Повторите команду");
                                }
                                break;
                            //Choose Consult
                            case State.ChooseConsult:
                                if(text == TEXT_BACK)
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите:", replyMarkup: GetButtons());
                                    _clientState[update.Message.Chat.Id] = null;
                                }
                                else
                                {
                                    switch (text)
                                    {
                                        case "Адрес и телефон":
                                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Г.Стерлитамак,ул.Ленина,12");
                                            _client.SendTextMessageAsync(update.Message.Chat.Id, "📱+79999999999", replyMarkup: GetButtons());
                                            _clientState[update.Message.Chat.Id] = null;
                                            break;
                                        case "Часы работы":
                                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Будние: с 9.00 - 19.00\nВыходные: с 10.00 - 15.00", replyMarkup: GetButtons());
                                            _clientState[update.Message.Chat.Id] = null;
                                            break;
                                        case "Как зовут директора?":
                                            _client.SendTextMessageAsync(update.Message.Chat.Id, "Петр Петров Петрович", replyMarkup: GetButtons());
                                            _clientState[update.Message.Chat.Id] = null;
                                            break;
                                        default:
                                            break;
                                    }
                                }                    
                                break;
                            //Users Request
                            case State.StartReq:
                                if(text == TEXT_BACK)
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите:", replyMarkup: GetButtons());
                                    _clientState[update.Message.Chat.Id] = null;
                                }
                                else if (text == TEXT_MY_REQ)
                                {
                                    var requests = GetRequests(update.Message.Chat.Id);

                                    foreach (var request in requests)
                                    {
                                        string answer1 = $"Фио - {request["ФИО"]}\nАдрес - {request["Адрес"]}\nНомер - {request["Номер"]}\nЧто произошло? - {request["Что произошло?"]}";
                                        _client.SendTextMessageAsync(update.Message.Chat.Id, answer1, replyMarkup:GetButtons());
                                    }
                                    _clientState[update.Message.Chat.Id] = null;
                                }
                                else if (text == TEXT_LEAVE_REQ)
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Давайте заполним заявку.");
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите ФИО, как к Вам обращаться?", replyMarkup: new ReplyKeyboardRemove());
                                    userRequets = new UserRequest();
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.ReqEnterFIO};
                                }
                                else
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Повторите команду");
                                }
                                break;
                            case State.ReqEnterFIO:
                                userRequets.FIO = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите адрес. Куда нужно будет подъехать?");

                                _clientState[update.Message.Chat.Id] = new UserState { State = State.ReqEnterAdress };
                                break;
                            case State.ReqEnterAdress:
                                userRequets.Adress = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите номер телефона, по которому можно будет созвониться.");

                                _clientState[update.Message.Chat.Id] = new UserState { State = State.ReqEnterNumber };
                                break;
                            case State.ReqEnterNumber:
                                userRequets.Number = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Напишите вкратце, что произошло. ");

                                _clientState[update.Message.Chat.Id] = new UserState { State = State.ReqEnterIncident };
                                break;
                            case State.ReqEnterIncident:
                                userRequets.Icident = text;
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Проверьте, все ли верно введено:");

                                string answer = $"Фио - {userRequets.FIO}\nАдрес - {userRequets.Adress}\nНомер - {userRequets.Number}\nЧто произошло? - {userRequets.Icident}";
                                _client.SendTextMessageAsync(update.Message.Chat.Id, answer, replyMarkup: GetYesNoButtons());
                                _clientState[update.Message.Chat.Id] = new UserState { State = State.ReqEnterYesNo };
                                break;
                            case State.ReqEnterYesNo:
                                if(text == "Да")
                                {
                                    PutRequest(userRequets);
                                    userRequets = null;
                                    
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Заявка успшено отправлена!\nОна будет обработана в ближайшее время", replyMarkup:GetButtons());
                                    _clientState[update.Message.Chat.Id] = null;
                                }
                                else
                                {
                                    text = TEXT_LEAVE_REQ;
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.StartReq };
                                    goto case State.StartReq;
                                }
                                break;
                            //Admin panel
                            case State.StartAdmin:
                                switch (text)
                                {
                                    case "Рассылка":
                                        _client.SendTextMessageAsync(update.Message.Chat.Id, "Введите текст, который Вы хотите разослать.", replyMarkup: new ReplyKeyboardRemove());
                                        _clientState[update.Message.Chat.Id] = new UserState { State = State.StartSendList };
                                        break;
                                    case "Колличество пользователей":
                                        int count_users = GetCountUsers();
                                        _client.SendTextMessageAsync(update.Message.Chat.Id, $"Колличество пользователей:\n{count_users}", replyMarkup: GetAdminButtons());
                                        break;
                                    case "Выход":
                                        _client.SendTextMessageAsync(update.Message.Chat.Id, $"Выберите:", replyMarkup: GetButtons());
                                        _clientState[update.Message.Chat.Id] = null;
                                        break;   
                                    default:
                                        break;
                                }
                                break;
                            case State.StartSendList:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Ваш текст:\n{text}");
                                _client.SendTextMessageAsync(update.Message.Chat.Id, $"Все верно?", replyMarkup: GetYesNoButtons());
                                _clientState[update.Message.Chat.Id] = new UserState { State = State.SendListYesNo };
                                break;
                            case State.SendListYesNo:
                                if(text == "Да")
                                {
                                    SendMessagesAllUsers();
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, $"Сообщение успешно разослано.", replyMarkup: GetAdminButtons());
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.StartAdmin };
                                }
                                else
                                {
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.StartSendList };
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (text)
                        {
                            case "/start":
                                if (!UserLogin(update.Message.Chat.Id))
                                {
                                    //Set state RegisterUser
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.RegisterUser };
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Для того чтобы мной пользоваться, необходимо зарегестрироваться.");
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Нужен только твой номер :)\n Введи его в формате 79*********");

                                }
                                else
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Добро пожаловать!\nЧем я могу помочь?", replyMarkup: GetButtons());
                                    _clientState[update.Message.Chat.Id] = null;
                                    Console.WriteLine(update.Message.Chat.Id);
                                }
                                
                                break;
                            case TEXT_CONSULT:
                                Console.WriteLine(update.Message.Chat.Id);
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите интересующий вопрос из списка:", replyMarkup: GetConsultButtons());
                                _clientState[update.Message.Chat.Id] = new UserState { State = State.ChooseConsult};
                                break;
                            case TEXT_REQ:
                                _client.SendTextMessageAsync(update.Message.Chat.Id, "Выберите то что Вам нужно:", replyMarkup: GetReqButtons());
                                _clientState[update.Message.Chat.Id] = new UserState { State = State.StartReq};
                                break;
                            case "/admin":
                                if (admins_id.Contains(update.Message.Chat.Id))
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Добро пожалость в панель администратора.\nВыберите необходимое действие", replyMarkup: GetAdminButtons());
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.StartAdmin };
                                }
                                else
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
                                }
                                break;
                            default:
                                if (!UserLogin(update.Message.Chat.Id))
                                {
                                    //Set state RegisterUser
                                    _clientState[update.Message.Chat.Id] = new UserState { State = State.RegisterUser };
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Для того чтобы мной пользоваться, необходимо зарегестрироваться.");
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Нужен только Ваш номер :)\n Введите его в формате 79*********");
                                }
                                else
                                {
                                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
                                }
                                
                                break;
                        }
                    }
                    break;
                default:
                    _client.SendTextMessageAsync(update.Message.Chat.Id, "Я не знаю такой команды.\nПовтори попытку.", replyMarkup: GetButtons());
                    break;

            }
        }

        private int GetCountUsers()
        {
            return -1;
        }

        private void SendMessagesAllUsers()
        {
            var a = GetUsers();
            foreach (var user in a)
            {
                Console.WriteLine(user);
            }
            Console.WriteLine("Acces!");
        }

        private List<long> GetUsers()
        {
            return new List<long>{ 1610733398 };
        }

        private IReplyMarkup? GetAdminButtons()
        {
            List<List<KeyboardButton>> buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>(){new KeyboardButton("Рассылка"), new KeyboardButton("Колличество пользователей") }
            };
            buttons.Add(new List<KeyboardButton> { new KeyboardButton("Выход") });
            var rmu = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
            return rmu;
        }

        private void PutRequest(UserRequest userRequets)
        {

            return;
        }

        private List<Dictionary<string, string>> GetRequests(long id)
        {
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>{
                    {"ФИО","Петрович"},
                    {"Адрес","Стерлитамак, ул ленина, 123" },
                    {"Номер","123123" },
                    {"Что произошло?","трубы горят" }
                }
            };

            return list;

        }

        private IReplyMarkup? GetReqButtons()
        {
            List<List<KeyboardButton>> buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>(){new KeyboardButton(TEXT_MY_REQ), new KeyboardButton(TEXT_LEAVE_REQ) }
            };
            buttons.Add(new List<KeyboardButton> { new KeyboardButton(TEXT_BACK) });
            var rmu = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
            return rmu;
        }

        private IReplyMarkup? GetConsultButtons()
        {
            List<List<KeyboardButton>> buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>(){new KeyboardButton("Адрес и телефон"), new KeyboardButton("Часы работы"), new KeyboardButton("Как зовут директора?") }
            };
            buttons.Add(new List<KeyboardButton> { new KeyboardButton(TEXT_BACK) });
            var rmu = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
            return rmu;
        }

        private IReplyMarkup? GetYesNoButtons()
        {
            List<List<KeyboardButton>> buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>(){new KeyboardButton("Да"), new KeyboardButton("Нет") }
            };
            var rmu = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
            return rmu;
        }

        private void RegisterUser(string? text)
        {
            Console.WriteLine("Регистрация прошла успешно!");
        }

        //check login user
        private bool UserLogin(long id)
        {
            //TODO
            return true;
        }

        private IReplyMarkup? GetButtons()
        {
            List<List<KeyboardButton>> buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>(){new KeyboardButton(TEXT_CONSULT), new KeyboardButton(TEXT_REQ) }
            };
            var rmu = new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true};
            return rmu;
        }
    }
}