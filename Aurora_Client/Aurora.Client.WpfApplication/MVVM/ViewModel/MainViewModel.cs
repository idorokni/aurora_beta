using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Aurora.Client.Communication.Managers;
using Aurora.Client.WpfApplication.Core;
using SharedData.Model;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public static MainViewModel Instance { get; private set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public static RelayCommand LastRelayCommand { get; set; }

        public RelayCommand EndProgramCommand { get; set; }
        public RelayCommand MinimizeProgramCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }


        public MainViewModel()
        {
            Instance = this;
            EndProgramCommand = new RelayCommand(async o =>
            {
                await Task.Delay(500);
                Environment.Exit(0);
            });
            MinimizeProgramCommand = new RelayCommand(async o => 
            {
                await Task.Delay(500);
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized; 
            });
            LastRelayCommand = null;
            RefreshCommand = new RelayCommand(async o =>
            {
                LastRelayCommand?.Execute(null);
            });
            HomeCommand = new RelayCommand(async o =>
            {
                if (HomeViewModel.Instance == null)
                {
                    return;
                }
                await Task.Delay(500);
                Instance.CurrentView = HomeViewModel.Instance;
            });
            InitializeViewModelAsync();
        }

        private async void InitializeViewModelAsync()
        {
            if(! await Communicator.Instance.ConnectToServerAsync())
            {
                return;
            }
            var responseInfo = await AuthenticationManager.Instance.TrySigninginToServerWithToken();
            if (responseInfo.code == ResponseCode.TOKEN_CONNECT_FAILED || string.IsNullOrEmpty(responseInfo.message))
            {
                CurrentView = new LoginViewModel();
            }
            else
            {
                var loggedUser = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(responseInfo.message);

                var tuple = await SocialMediaManager.Instance.GetRecentPost(0);
                var onlineUsers = await SocialMediaManager.Instance.GetOnlineUsersAsync();
                MainViewModel.Instance.CurrentView = new HomeViewModel(loggedUser, new DefultViewPostModel
                {
                    PostID = tuple.Item2,
                    Image = Convert.FromBase64String(tuple.Item3),
                    UserID = tuple.Item1
                }, onlineUsers);

            }
            _ = Task.Run(() => HandleServerMessages());
        }

        public async Task HandleServerMessages()
        {
            while (true)
            {
                RequestInfo request = await Communicator.Instance.ReadMessageFromServerUpdate(Communicator.Instance.ServerConnect);
                await HandleUpdate(request);
            }
        }

        private async Task HandleUpdate(RequestInfo info)
        {
            switch (info.code)
            {
                case RequestCode.SEND_MESSAGE_REQUEST_CODE:
                    await HandleChatsUpade(info);
                    break;
                case RequestCode.LOGIN_REQUEST_CODE:
                    await HandleNewOnlineUser(info);
                    break;
                case RequestCode.SIGN_UP_REQUEST_CODE:
                    await HandleNewOnlineUser(info);
                    break;
                case RequestCode.CONNECT_REQUEST_CODE:
                    await HandleNewOnlineUser(info);
                    break;
                default:
                    break;
            }
        }

        private async Task HandleNewOnlineUser(RequestInfo info)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<(UserData, string, int)>(info.message);
            Application.Current.Dispatcher.Invoke(() =>
            {
                HomeViewModel.Instance.OnlineFollowingUsers.Add(new UserChatModel
                {
                    UserID = user.Item3,
                    Username = user.Item1.Username,
                    Image = (user.Item1.ProfilePicture == null || user.Item1.ProfilePicture.Length == 0)
                        ? File.ReadAllBytes("Images/default_user_image.png")
                        : Convert.FromBase64String(user.Item1.ProfilePicture),
                });
            });
        }

        private async Task HandleChatsUpade(RequestInfo info)
        {
            var chat = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatData>(info.message);
            if(chat is null)
            {
                return;
            }

            if(UserChatsViewModel.Instance.Chats.ContainsKey(chat.SenderID) == false)
            {
                UserChatsViewModel.Instance.Chats.Add(chat.SenderID, new ObservableCollection<MessageModel>());
                var data = await SocialMediaManager.Instance.GetUserData(chat.SenderID);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserChatsViewModel.Instance.Users.Add(new UserChatModel
                    {
                        UserID = chat.SenderID,
                        Username = data.Username,
                        Image = (data.ProfilePicture == null || data.ProfilePicture.Length == 0)
                            ? File.ReadAllBytes("Images/default_user_image.png")
                            : Convert.FromBase64String(data.ProfilePicture),
                    });
                });
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                UserChatsViewModel.Instance.Chats[chat.SenderID].Add(new MessageModel
                {
                    IsSender = false,
                    Content = chat.Message,
                });
            });

            HomeViewModel.Instance.UnreadMessageCount++;
        }
    }
}
