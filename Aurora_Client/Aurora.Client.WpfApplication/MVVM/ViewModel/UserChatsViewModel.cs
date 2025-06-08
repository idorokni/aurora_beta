using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Aurora.Client.Communication.Managers;
using Aurora.Client.WpfApplication.Core;
using SharedData.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    public class UserChatsViewModel : ObservableObject
    {
        private static UserChatsViewModel _instance;
        private string _username = "";
        private byte[] _profilePicture = null!;
        private int _currentUserID = 0;
        private ObservableCollection<MessageModel> _currentChat = new();
        private ObservableCollection<UserChatModel> _users = new();
        public string Username { get { return _username; } set { _username = value; OnPropertyChanged(); } }
        public byte[] ProfilePicture { get { return _profilePicture; } set { _profilePicture = value; OnPropertyChanged(); } }
        public Dictionary<int, ObservableCollection<MessageModel>> Chats { get; set; } = new();
        public ObservableCollection<MessageModel> CurrentChat { get { return _currentChat; } set { _currentChat = value; OnPropertyChanged(); } }
        public ObservableCollection<UserChatModel> Users { get { return _users; } set { _users = value; OnPropertyChanged(); } }
        public string MessageText { get; set; }

        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand SwapUserCommand { get; set; }

        public static UserChatsViewModel Instance
        {
            get
            {
                return _instance ??= new UserChatsViewModel();
            }
        }

        public UserChatsViewModel()
        {
            SendMessageCommand = new RelayCommand(async o =>
            {
                if (!await SocialMediaManager.Instance.SendMessage(SocialMediaManager.Instance.UserID, _currentUserID, MessageText ?? string.Empty))
                {
                    MessageText = "";
                    return;
                }

                CurrentChat ??= new ObservableCollection<MessageModel>();
                CurrentChat.Add(new MessageModel
                {
                    Content = MessageText,
                    IsSender = true,
                });

                if(!Chats.ContainsKey(_currentUserID))
                {
                    Chats.Add(_currentUserID, CurrentChat);
                }

                if(Users.FirstOrDefault(u => u.UserID == _currentUserID) == null)
                {
                    Users.Add(new UserChatModel
                    {
                        UserID = _currentUserID,
                        Username = Username,
                        Image = ProfilePicture
                    });
                }
            });

            SwapUserCommand = new RelayCommand(o =>
            {
                if (o is UserChatModel user)
                {
                    LoadChat(user.UserID, user.Username, user.Image);
                    MainViewModel.LastRelayCommand = SwapUserCommand;
                    MainViewModel.LastRelayCommandParameters = user;
                }
            });
        }

        public void LoadChat(int userID, string username, byte[] profilePicture)
        {
            if(userID == -1)
            {
                var firstChat = Chats.FirstOrDefault();
                var selectedUser = Users.FirstOrDefault(u => u.UserID == firstChat.Key);
                CurrentChat = Chats.FirstOrDefault().Value;
                Username = selectedUser.Username;
                ProfilePicture = selectedUser.Image;
                _currentUserID = firstChat.Key;
            }
            CurrentChat = Chats.ContainsKey(userID) ? Chats[userID] : null;
            Username = username;
            ProfilePicture = profilePicture;
            _currentUserID = userID;
        }


    }
}
