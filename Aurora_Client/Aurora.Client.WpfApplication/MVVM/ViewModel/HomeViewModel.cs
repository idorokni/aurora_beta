using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Aurora.Client.Communication.Managers;
using Aurora.Client.WpfApplication.Core;
using SharedData.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        private static HomeViewModel _instance;
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

        public ObservableCollection<UserChatModel> OnlineFollowingUsers { get; set; } = new ObservableCollection<UserChatModel>();

        public static HomeViewModel Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        private int _unreadMessageCount;
        public int UnreadMessageCount
        {
            get => _unreadMessageCount;
            set
            {
                _unreadMessageCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasUnreadMessages));
            }
        }

        public bool HasUnreadMessages => UnreadMessageCount > 0;

        public int UserID { get; set; }

        public RelayCommand SwitchToChangeProfile { get; set; }
        public RelayCommand SwitchToCreateNewPost { get; set; }
        public RelayCommand SearchUsers { get; set; }
        public RelayCommand SwitchToMessagesCommand { get; set; }
        public RelayCommand GetUserCommand { get;set; }

        public HomeViewModel(int userID, DefultViewPostModel post, List<Tuple<int, string, string>> onlineFollowingUsers)
        {
            SocialMediaManager.Instance.UserID = userID;
            Instance = this;
            CurrentView = new DefaultInsideHomeViewModel(post);
            SwitchToChangeProfile = new RelayCommand(async o =>
            {            
                var user = await SocialMediaManager.Instance.GetUserData();
                MainViewModel.Instance.CurrentView = new EditProfileViewModel(user);

                MainViewModel.LastRelayCommand = SwitchToChangeProfile;
            });

            SwitchToCreateNewPost = new RelayCommand(o =>
            {
                MainViewModel.Instance.CurrentView = new CreateNewPostViewModel();
                MainViewModel.LastRelayCommand = SwitchToCreateNewPost;
            });

            SearchUsers = new RelayCommand(async o =>
            {
                CurrentView = new SearchResultViewModel(SearchQuery ?? "");
                MainViewModel.LastRelayCommand = SearchUsers;
            });

            SwitchToMessagesCommand = new RelayCommand(o =>
            {
                MainViewModel.Instance.CurrentView = UserChatsViewModel.Instance;
                UnreadMessageCount = 0;
                MainViewModel.LastRelayCommand = SwitchToMessagesCommand;
            });

            GetUserCommand = new RelayCommand(async o =>
            {
                if (o is UserChatModel user)
                {
                    var result = await SocialMediaManager.Instance.GetUserData(user.UserID);
                    var posts = await SocialMediaManager.Instance.GetAllPostsAsync(user.UserID);
                    MainViewModel.Instance.CurrentView = new UserProfileViewModel(user.UserID, result, posts);
                    MainViewModel.LastRelayCommand = GetUserCommand;
                    MainViewModel.LastRelayCommandParameters = user;
                }
            });

            onlineFollowingUsers?.ForEach(u => OnlineFollowingUsers.Add(new UserChatModel
            {
                Username = u.Item2,
                UserID = u.Item1,
                Image = u.Item3 == null
                    ? File.ReadAllBytes("Images/default_user_image.png")
                    : Convert.FromBase64String(u.Item3)
            }));
        }

    }
}
