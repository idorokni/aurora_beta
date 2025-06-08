using Aurora.Client.WpfApplication.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.Infrustructure;
using SharedData.Model;
using Aurora.Client.Communication.Managers;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    public class UserProfileViewModel : ObservableObject
    {
        private static readonly SemaphoreSlim 
            _semaphoreSlim = new(1, 1);
        private byte[] _profileImage;

        private UserData _user;
        private int _followers = 0;
        private int _following = 0;
        public int Followers { get { return _followers; } set { _followers = value; OnPropertyChanged(); } }
        public int Following { get { return _following; } set { _following = value; OnPropertyChanged(); } }
        public UserData User { get { return _user; } set { _user = value; OnPropertyChanged(); } }
        private int _userID;
        public byte[] ProfileImage { get { return _profileImage; } set { _profileImage = value; OnPropertyChanged(); } }
        public ObservableCollection<PostModel> UserPosts { get; set; } = new();
        
        public RelayCommand FollowUserCommand { get; set; }
        public RelayCommand UnfollowUserCommand { get; set; }
        public RelayCommand ViewPostCommand { get; set; }
        public RelayCommand StartChat { get; set; }

        public UserProfileViewModel(int userID, UserData userData, List<PostModel> posts)
        {
            User = userData;
            _userID = userID;
            ProfileImage = (User.ProfilePicture == null || User.ProfilePicture.Length == 0)
                ? File.ReadAllBytes("Images/default_user_image.png")
                : Convert.FromBase64String(User.ProfilePicture);
            Followers = userData.Followers;
            Following = userData.Following;

            ViewPostCommand = new RelayCommand(async o =>
            {
                if (o is PostModel post)
                {
                    var data = await SocialMediaManager.Instance.GetPostData(post.PostID);
                    MainViewModel.Instance.CurrentView = new PostViewModel(data, post, userData);

                    MainViewModel.LastRelayCommand = ViewPostCommand!;
                    MainViewModel.LastRelayCommandParameters = post;
                }
            });

            FollowUserCommand = new RelayCommand(async o =>
            {
                if (await SocialMediaManager.Instance.FollowUser(userID))
                {
                    Followers++;
                }
            });

            UnfollowUserCommand = new RelayCommand(async o =>
            {
                if (await SocialMediaManager.Instance.UnfollowUser(userID))
                {
                    Followers--;
                }
            });
            StartChat = new RelayCommand(o =>
            {
                UserChatsViewModel.Instance.LoadChat(userID, _user.Username, ProfileImage);
                MainViewModel.Instance.CurrentView = UserChatsViewModel.Instance;
                MainViewModel.LastRelayCommand = StartChat;
            });
            posts.ForEach(post => UserPosts.Add(post));

        }
    }
}
