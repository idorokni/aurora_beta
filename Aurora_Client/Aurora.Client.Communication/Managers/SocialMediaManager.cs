using Aurora.Client.Communication.Codes;
using Aurora.Client.Communication.DataStruct;
using Aurora.Client.Communication.Infrustructure;
using SharedData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Managers
{
    public class SocialMediaManager
    {
        private static SocialMediaManager _instance = null!;
        public static SocialMediaManager Instance
        {
            get
            {
                _instance ??= new SocialMediaManager();
                return _instance;
            }
        }

        public int UserID { get; set; } = 0;

        public async Task<List<PostModel>> GetAllPostsAsync(int userID)
        {
            var postAmount = await GetAmountOfPostsAsync(userID);
            var posts = new List<PostModel>(); // Use List instead of ConcurrentBag
            var tasks = new List<Task>();

            if (postAmount == 0) return [];

            var lockObject = new object(); // Protect the list

            for (int index = 0; index < postAmount; index++)
            {
                var post = await GetPostAsync(userID, index);
                if (post != null)
                {
                    lock (lockObject) posts.Add(post);
                }
            }

            await Task.WhenAll(tasks);
            return posts;
        }

        private async Task<int> GetAmountOfPostsAsync(int userID)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_AMOUNT_OF_POSTS_REQUEST_CODE,
                message = userID.ToString()
            });

            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            if (responseInfo.code == ResponseCode.GET_AMOUNT_OF_POSTS_FAILED)
            {
                return 0;
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<int>(responseInfo.message);
        }

        private async Task<PostModel> GetPostAsync(int userID, int index)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_POST_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject((index, userID))
            });

            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            if (responseInfo.code == ResponseCode.GET_POST_FAILED)
            {
                return null;
            }

            var post = Newtonsoft.Json.JsonConvert.DeserializeObject<Tuple<int, string>>(responseInfo.message);
            return new PostModel
            {
                Image = Convert.FromBase64String(post.Item2),
                PostID = post.Item1
            };
        }

        public async Task<UserData> GetUserData()
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_USER_DATA_REQUEST_CODE,
                message = UserID.ToString()
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            if (responseInfo.code == ResponseCode.GET_USER_DATA_FAILED)
            {
                return null;
            }
            return JsonSerializer.Deserialize<UserData>(responseInfo.message);
        }

        public async Task<UserData> GetUserData(int userID)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_USER_DATA_REQUEST_CODE,
                message = userID.ToString()
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            if (responseInfo.code == ResponseCode.GET_USER_DATA_FAILED)
            {
                return null;
            }
            return JsonSerializer.Deserialize<UserData>(responseInfo.message);
        }

        public async Task<Tuple<int, int, string>> GetRecentPost(int index)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_RECENT_POSTS_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(index)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            var tuple = Newtonsoft.Json.JsonConvert.DeserializeObject<Tuple<int, int, string>>(responseInfo.message);

            return tuple;
        }

        public async Task<Tuple<int, int, string>> GetFollowingPost(int index)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_FOLLOWING_POSTS_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(index)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            var tuple = Newtonsoft.Json.JsonConvert.DeserializeObject<Tuple<int, int, string>>(responseInfo.message);
            return tuple;
        }

        public async Task<List<Tuple<int, string, string>>> GetOnlineUsersAsync()
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_ONLINE_USERS_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(0)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            var onlineUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Tuple<int, string, string>>>(responseInfo.message);

            return onlineUsers;
        }

        public async Task<bool> UpdateUser(string bio, string username, string birthday, string email, int followers, int following, string joinDate, string imagePath)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.UPDATE_USER_DATA_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Bio = bio,
                    Username = username,
                    Birthday = birthday,
                    Email = email,
                    Followers = followers,
                    Following = following,
                    JoinDate = joinDate,
                    ProfilePicture = await File.ReadAllBytesAsync(imagePath)
                })
            });

            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return responseInfo.code == ResponseCode.UPDATE_USER_DATE_SUCCESS;
        }

        public async Task<List<SearchModel>> SearchQueryAsync(string query)
        {
            try
            {
                await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
                {
                    code = RequestCode.SEARCH_USER_REQUEST_CODE,
                    message = query
                });

                var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
                if (responseInfo.code == ResponseCode.SEARCH_USER_SUCCESS)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchModel>>(responseInfo.message) ?? [];
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<PostData> GetPostData(int data)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.GET_POST_DATA_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(data)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            if (responseInfo.code == ResponseCode.GET_USER_DATA_FAILED)
            {
                return new();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PostData>(responseInfo.message) ?? new();
        }

        public async Task<bool> LikePost(int postID, int likeKind)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.LIKE_POST_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(new Tuple<int, int>(postID, likeKind))
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return responseInfo.code == ResponseCode.LIKE_POST_SUCCESS;
        }

        public async Task CommentPost(int postID, string comment)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.COMMENT_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(new Tuple<int, string>(postID, comment))
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
        }

        public async Task<bool> SendMessage(int senderID, int reciverID, string messageContent)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.SEND_MESSAGE_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(new ChatData
                {
                    SenderID = senderID,
                    ReceiverID = reciverID,
                    Message = messageContent,
                })
            });


            ResponseInfo info = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return info.code == ResponseCode.SEND_MESSAGE_SUCCESS;
        }

        public async Task<bool> FollowUser(int userID)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.FOLLOW_USER_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(userID)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return responseInfo.code == ResponseCode.FOLLOW_USER_SUCCESS;
        }

        public async Task<bool> UnfollowUser(int userID)
        {
            await Communicator.Instance.SendMessageToServerEncrypted(Communicator.Instance.Client, new RequestInfo
            {
                code = RequestCode.UNFOLLOW_USER_REQUEST_CODE,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(userID)
            });
            var responseInfo = await Communicator.Instance.ReadMessageFromServerEncrypted(Communicator.Instance.Client);
            return responseInfo.code == ResponseCode.UNFOLLOW_USER_SUCCESS;
        }
    }
}
