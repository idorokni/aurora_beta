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


    public class DefaultInsideHomeViewModel : ObservableObject
    {
        private int followingPostsIndex = 0;
        private int recentPostsIndex = 0;

        bool isRecentPosts = true;
        private DefultViewPostModel mainPost = new();

        public DefultViewPostModel MainPost { get => mainPost; set { mainPost = value; OnPropertyChanged(); } }

        public RelayCommand DefaultViewPostCommand { get; set; }
        public RelayCommand GetRecentPostsCommand { get; set; }
        public RelayCommand GetFollowingPostsCommand { get; set; }

        public RelayCommand SwipeLeftRelayCommand { get; set; }
        public RelayCommand SwipeRightRelayCommand { get; set; }

        public DefaultInsideHomeViewModel(DefultViewPostModel post)
        {
            MainPost = post;

            DefaultViewPostCommand = new RelayCommand(async o =>
            {
                var data = await SocialMediaManager.Instance.GetPostData(MainPost.PostID);
                MainViewModel.Instance.CurrentView = new PostViewModel(data, new PostModel
                {
                    PostID = MainPost.PostID,
                    Image = MainPost.Image
                }, await SocialMediaManager.Instance.GetUserData(MainPost.UserID));

                MainViewModel.LastRelayCommand = DefaultViewPostCommand;
            });

            GetRecentPostsCommand = new RelayCommand(async o =>
            {
                isRecentPosts = true;

                var tuple = await SocialMediaManager.Instance.GetRecentPost(recentPostsIndex);
                if(tuple == null)
                {
                    recentPostsIndex--;
                    return;
                }
                MainPost = new DefultViewPostModel
                {
                    PostID = tuple.Item2,
                    Image = Convert.FromBase64String(tuple.Item3),
                    UserID = tuple.Item1
                };
            });

            GetFollowingPostsCommand = new RelayCommand(async o =>
            {
                isRecentPosts = false;

                var tuple = await SocialMediaManager.Instance.GetFollowingPost(followingPostsIndex);
                if (tuple == null && followingPostsIndex == 0)
                {
                    MainPost = new DefultViewPostModel
                    {
                        PostID = 0,
                        Image = null,
                        UserID = 0
                    };

                    return;
                }
                if (tuple == null)
                {
                    followingPostsIndex--;
                    return;
                }
                MainPost = new DefultViewPostModel
                {
                    PostID = tuple.Item2,
                    Image = Convert.FromBase64String(tuple.Item3),
                    UserID = tuple.Item1
                };
            });

            SwipeRightRelayCommand = new RelayCommand(async o =>
            {
                if (isRecentPosts)
                {
                    GetRecentPostsCommand.Execute(++recentPostsIndex);
                }
                else
                {
                    GetFollowingPostsCommand.Execute(++followingPostsIndex);
                }
            });

            SwipeLeftRelayCommand = new RelayCommand(async o =>
            {

                if (isRecentPosts)
                {
                    recentPostsIndex = recentPostsIndex == 0 ? 0 : recentPostsIndex - 1;
                    GetRecentPostsCommand.Execute(o);
                }
                else
                {
                    followingPostsIndex = followingPostsIndex == 0 ? 0 : followingPostsIndex - 1;
                    GetFollowingPostsCommand.Execute(o);
                }
            });
        }
    }
}
