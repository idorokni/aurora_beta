using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using Aurora.Client.Communication.Managers;
using Aurora.Client.WpfApplication.Core;
using SharedData.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aurora.Client.WpfApplication.MVVM.ViewModel
{
    public class PostViewModel : ObservableObject
    {
        private byte[] _post;
        private byte[] _profilePicture;
        private string _username;
        private string _email;
        private int _amountOfLikes;
        private int _amountOfDislikes;
        private int _amountOfSuperLikes;
        private bool _alreadyLiked = false;
        private bool _alreadyDisliked = false;
        private bool _alreadySuperLiked = false;
        private string _description = string.Empty;

        public byte[] Post { get { return _post; } set { _post = value; OnPropertyChanged(); } }

        public byte[] ProfilePicture { get { return _profilePicture; } set { _profilePicture = value; OnPropertyChanged(); } }

        public string Username { get { return _username; } set { _username = value; OnPropertyChanged(); } }

        public string Email { get { return _email; } set { _email = value; OnPropertyChanged(); } }

        public int AmountOfLikes { get { return _amountOfLikes; } set { _amountOfLikes = value; OnPropertyChanged(); } }

        public int AmountOfDislikes { get { return _amountOfDislikes; } set { _amountOfDislikes = value; OnPropertyChanged(); } }

        public int AmountOfSuperLikes { get { return _amountOfSuperLikes; } set { _amountOfSuperLikes = value; OnPropertyChanged(); } }

        public bool AlreadyLiked { get { return _alreadyLiked; } set { _alreadyLiked = value; OnPropertyChanged(); } }

        public bool AlreadyDisliked { get { return _alreadyDisliked; } set { _alreadyDisliked = value; OnPropertyChanged(); } }

        public bool AlreadySuperLiked { get { return _alreadySuperLiked; } set { _alreadySuperLiked = value; OnPropertyChanged(); } }

        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(); } }

        public ObservableCollection<CommentModel> Comments { get; set; } = new ObservableCollection<CommentModel>();

        public RelayCommand LikeCommand { get; set; }
        public RelayCommand CommentCommand { get; set; }
        public RelayCommand SuperlikeCommand { get; set; }
        public RelayCommand DislikeCommand { get; set; }

        public string CommentData { get; set; }




        public PostViewModel(PostData postData, PostModel viewData, UserData userData)
        {
            Post = viewData.Image;
            ProfilePicture = (userData.ProfilePicture == null || userData.ProfilePicture.Length == 0)
                ? File.ReadAllBytes("Images/default_user_image.png")
                : Convert.FromBase64String(userData.ProfilePicture);
            Username = userData.Username;
            Email = userData.Email;
            AmountOfLikes = postData.AmountOfLikes;
            AmountOfDislikes = postData.AmountOfDislikes;
            AmountOfSuperLikes = postData.AmountOfSuperLikes;
            AlreadyLiked = postData.AlreadyLiked;
            AlreadyDisliked = postData.AlreadyDisliked;
            AlreadySuperLiked = postData.AlreadySuperLiked;
            Description = postData.Description;

            LoadComments(postData.Comments);


            CommentCommand = new RelayCommand(async o =>
            {
                await SocialMediaManager.Instance.CommentPost(viewData.PostID, CommentData);
            });

            LikeCommand = new RelayCommand(async o =>
            {
                if (await SocialMediaManager.Instance.LikePost(viewData.PostID, 1))
                {
                    if (AlreadyLiked)
                    {
                        AmountOfLikes--;
                    }
                    else
                    {
                        AmountOfLikes++;
                    }
                    AlreadyLiked = !AlreadyLiked;
                }
            });

            DislikeCommand = new RelayCommand(async o =>
            {
                if (await SocialMediaManager.Instance.LikePost(viewData.PostID, 2))
                {
                    if(AlreadyDisliked)
                    {
                        AmountOfDislikes--;
                    }
                    else
                    {
                        AmountOfDislikes++;
                    }
                    AlreadyDisliked = !AlreadyDisliked;
                }
            });

            SuperlikeCommand = new RelayCommand(async o =>
            {
                if (await SocialMediaManager.Instance.LikePost(viewData.PostID, 3))
                {

                    if (AlreadySuperLiked)
                    {
                        AmountOfSuperLikes--;
                    }
                    else
                    {
                        AmountOfSuperLikes++;
                    }
                    AlreadySuperLiked = !AlreadySuperLiked;
                }
            });
        }

        private void LoadComments(List<CommentData> comments)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Comments.Clear();
                    comments.ForEach(comment =>
                    {
                        Comments.Add(new CommentModel
                        {
                            CommentContent = comment.CommentContent,
                            ProfilePicture = (comment.ProfilePicture == null || comment.ProfilePicture.Length == 0)
                                ? File.ReadAllBytes("Images/default_user_image.png")
                                : Convert.FromBase64String(comment.ProfilePicture),
                            Username = comment.Username,
                            Email = comment.Email
                        });
                    });
                });
            });

        }
    }
}
