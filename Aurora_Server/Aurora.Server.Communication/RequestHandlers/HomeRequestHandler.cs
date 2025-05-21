using Aurora.Server.Communication.Codes;
using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aurora.Server.Communication.DataStruct;
using Aurora.Server.Communication.Services;
using Newtonsoft.Json;
using Aurora.Server.Communication.Infrustructure;

namespace Aurora.Server.Communication.RequestHandlers
{
    public class HomeRequestHandler(int _userID) : IRequestHandler
    {
        public int UserID => _userID;
        public bool IsRequestValid(RequestInfo info)
        {
            return info.code >= RequestCode.LOGIN_REQUEST_CODE && info.code <= RequestCode.SEND_MESSAGE_REQUEST_CODE;
        }

        public async Task<(IRequestHandler, ResponseInfo)> HandleRequest(RequestInfo info)
        {
            ResponseInfo responseInfo = new ResponseInfo();

            switch (info.code)
            {
                case RequestCode.GET_USER_DATA_REQUEST_CODE:
                    responseInfo = await HandleGetDataRequest(info);
                    break;
                case RequestCode.ADD_POST_REQUEST_CODE:
                        responseInfo = await HandlePostRequest(info);
                    break;
                case RequestCode.UPDATE_USER_DATA_REQUEST_CODE:
                    responseInfo = await HandleUploadDataRequest(info);
                    break;
                case RequestCode.SEARCH_USER_REQUEST_CODE:
                    responseInfo = await HandleSearchRequest(info);
                    break;
                case RequestCode.GET_AMOUNT_OF_POSTS_REQUEST_CODE:
                    responseInfo = await HandleGetAmountOfPostsRequest(info);
                    break;
                case RequestCode.GET_POST_REQUEST_CODE:
                    responseInfo = await HandleGetPost(info);
                    break;
                case RequestCode.GET_POST_DATA_REQUEST_CODE:
                    responseInfo = await HandleGetPostData(info);
                    break;
                case RequestCode.COMMENT_REQUEST_CODE:
                    responseInfo = await HandleCommentRequest(info);
                    break;
                case RequestCode.LIKE_POST_REQUEST_CODE:
                    responseInfo = await HandleLikePostRequest(info);
                    break;
                case RequestCode.FOLLOW_USER_REQUEST_CODE:
                    responseInfo = await HandleFollowUserRequest(info);
                    break;
                case RequestCode.UNFOLLOW_USER_REQUEST_CODE:
                    responseInfo = await HandleUnfollowUserRequest(info);
                    break;
                case RequestCode.GET_RECENT_POSTS_REQUEST_CODE:
                    responseInfo = await HandleGetRecentPostsRequest(info);
                    break;
                case RequestCode.GET_FOLLOWING_POSTS_REQUEST_CODE:
                    responseInfo = await HandleGetFollowingPostsRequest(info);
                    break;
                case RequestCode.GET_ONLINE_USERS_REQUEST_CODE:
                    responseInfo = await HandleGetOnlineUsersRequest(info);
                    break;
                case RequestCode.SEND_MESSAGE_REQUEST_CODE:
                    responseInfo = await HandleSendMessageRequest(info);
                    break;
                default:
                    break;
            }

            return (this, responseInfo);

        }

        private async Task<ResponseInfo> HandleSendMessageRequest(RequestInfo info)
        {
            return new ResponseInfo
            {
                code = ResponseCode.SEND_MESSAGE_SUCCESS,
                message = ""
            };
        }

        private async Task<ResponseInfo> HandleGetOnlineUsersRequest(RequestInfo info)
        {
            var onlineUsers = (await DatabaseManager.Instance.GetFollowingUsers(_userID))
                                    .Where(i => Communicator.Instance.Clients
                                    .Values
                                    .OfType<HomeRequestHandler>()
                                    .Any(handler => handler.UserID == i.Item1));


            return onlineUsers != null ? new ResponseInfo
            {
                code = ResponseCode.GET_ONLINE_USERS_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(onlineUsers)
            } : 
            new ResponseInfo
            {
                code = ResponseCode.GET_ONLINE_USERS_FAILED,
                message = ""
            };


        }

        private async Task<ResponseInfo> HandleGetRecentPostsRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(info.data);
            var post = await DatabaseManager.Instance.GetRecentPosts(data);
            return post != null ? new ResponseInfo
            {
                code = ResponseCode.GET_RECENT_POSTS_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(post)
            } :
            new ResponseInfo
            {
                code = ResponseCode.GET_RECENT_POSTS_FAILED,
                message = ""
            };
             
        }

        private async Task<ResponseInfo> HandleGetFollowingPostsRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(info.data);
            var post = await DatabaseManager.Instance.GetFollowingPosts(_userID, data);
            return post != null ? new ResponseInfo
            {
                code = ResponseCode.GET_FOLLOWING_POSTS_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(post)
            } :
            new ResponseInfo
            {
                code = ResponseCode.GET_FOLLOWING_POSTS_FAILED,
                message = ""
            };

        }

        private async Task<ResponseInfo> HandleFollowUserRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(info.data);

            return await DatabaseManager.Instance.FollowUser(_userID, data) ? new ResponseInfo
            {
                code = ResponseCode.FOLLOW_USER_SUCCESS,
                message = ""
            } : 
            new ResponseInfo
            {
                code = ResponseCode.FOLLOW_USER_FAILED,
                message = ""
            };

        }

        private async Task<ResponseInfo> HandleUnfollowUserRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(info.data);

            return await DatabaseManager.Instance.UnfollowUser(_userID, data) ? new ResponseInfo
            {
                code = ResponseCode.UNFOLLOW_USER_SUCCESS,
                message = ""
            } :
            new ResponseInfo
            {
                code = ResponseCode.UNFOLLOW_USER_FAILED,
                message = ""
            };

        }

        private async Task<ResponseInfo> HandleLikePostRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Tuple<int, int>>(info.data);
            var likeData = await DatabaseManager.Instance.GetUserLikeData(_userID, data.Item1);
            var likeDataAsList = new List<bool> { likeData.Item1, likeData.Item2, likeData.Item3 };
            if (likeDataAsList[data.Item2 - 1])
            {
                await DatabaseManager.Instance.RemoveLike(_userID, data.Item1, data.Item2);
            }
            else
            {
                await DatabaseManager.Instance.AddLike(_userID, data.Item1, data.Item2);
            }

            return new ResponseInfo
            {
                code = ResponseCode.LIKE_POST_SUCCESS,
                message = ""
            };
        }

        private async Task<ResponseInfo> HandleGetPostData(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(info.data);
            var post = await DatabaseManager.Instance.GetPostData(_userID, data);
            if (post == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.GET_POST_FAILED,
                    message = ""
                };
            }

            return new ResponseInfo
            {
                code = ResponseCode.GET_POST_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(post)
            };
        }

        private async Task<ResponseInfo> HandleCommentRequest(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Tuple<int, string>>(info.data);
            await DatabaseManager.Instance.AddComment(_userID, data.Item1, data.Item2);

            return new ResponseInfo
            {
                code = ResponseCode.COMMENT_SUCCESS,
                message = ""
            };
        }

        private async Task<ResponseInfo> HandleGetAmountOfPostsRequest(RequestInfo info)
        {
            int userID = int.Parse(info.data);
            var amount = await DatabaseManager.Instance.GetAmountOfPosts(userID);
            return new ResponseInfo
            {
                code = ResponseCode.GET_AMOUNT_OF_POSTS_SUCCESS,
                message = amount.ToString()
            };
        }

        private async Task<ResponseInfo> HandleGetPost(RequestInfo info)
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<(int, int)>(info.data);
            var post = await DatabaseManager.Instance.GetPost(data.Item2, data.Item1);
            if (post.Item2 == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.GET_POST_FAILED,
                    message = ""
                };
            }

            return new ResponseInfo
            {
                code = ResponseCode.GET_POST_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(post)
            };
        }

        private async Task<ResponseInfo> HandleGetDataRequest(RequestInfo info)
        {
            var userID = int.Parse(info.data);
            var userData = await DatabaseManager.Instance.GetUser(userID);

            if (userData == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.GET_USER_DATA_FAILED,
                    message = ""
                };
            }

            return new ResponseInfo
            {
                code = ResponseCode.GET_USER_DATA_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(userData)
            };
        }

        private async Task<ResponseInfo> HandleUploadDataRequest(RequestInfo info)
        {
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<ClientUserData>(info.data);
            var path = await ImageStorageService.SaveProfileImage(user.Username, user.ProfilePicture);

            var databaseUser = await DatabaseManager.Instance.GetUser(_userID);

            await DatabaseManager.Instance.UpdateUser(databaseUser.Username, user.Email, user.Bio, user.Birthday, path);
            databaseUser.Birthday = user.Birthday;
            databaseUser.ProfilePicture = path;
            databaseUser.Bio = user.Bio;
            databaseUser.Email = user.Email;

            return new ResponseInfo
            {
                code = ResponseCode.UPDATE_USER_DATE_SUCCESS,
                message = ""
            };


        }

        private async Task<ResponseInfo> HandleSearchRequest(RequestInfo info)
        {
            var searchQuery = info.data;
            if (searchQuery == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.SEARCH_USER_FAILED,
                    message = ""
                };
            }

            var users = DatabaseManager.Instance.SearchUsers(searchQuery);
            if (users == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.SEARCH_USER_FAILED,
                    message = ""
                };
            }

            return new ResponseInfo
            {
                code = ResponseCode.SEARCH_USER_SUCCESS,
                message = Newtonsoft.Json.JsonConvert.SerializeObject(users)
            };
        }

        private async Task<ResponseInfo> HandlePostRequest(RequestInfo info)
        {
            var newPostData = Newtonsoft.Json.JsonConvert.DeserializeObject<AddPostData>(info.data);
            if (newPostData == null)
            {
                return new ResponseInfo
                {
                    code = ResponseCode.ADD_POST_FAILED,
                    message = ""
                };
            }

            var postPath = await Services.ImageStorageService.SaveImageAsync(_userID.ToString(), newPostData.ImageData);
            await DatabaseManager.Instance.AddPost(_userID, newPostData.Description, postPath);

            return new ResponseInfo
            {
                code = ResponseCode.ADD_POST_SUCCESS,
                message = ""
            };

        }
    }
}
