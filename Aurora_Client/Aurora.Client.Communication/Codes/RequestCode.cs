using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Codes
{
    public enum RequestCode : byte
    {
        PORT_SEND_REQUEST_CODE = 30,
        LOGIN_REQUEST_CODE = 51,
        SIGN_UP_REQUEST_CODE = 52,
        CONNECT_REQUEST_CODE = 53,
        ADD_POST_REQUEST_CODE = 54,
        GET_USER_DATA_REQUEST_CODE = 55,
        UPDATE_USER_DATA_REQUEST_CODE = 56,
        SEARCH_USER_REQUEST_CODE = 57,
        GET_POST_REQUEST_CODE = 58,
        GET_AMOUNT_OF_POSTS_REQUEST_CODE = 59,
        GET_POST_DATA_REQUEST_CODE = 60,
        LIKE_POST_REQUEST_CODE = 61,
        TAG_POST_REQUEST_CODE = 62,
        COMMENT_REQUEST_CODE = 63,
        REFRESH_REQUEST_CODE = 64,
        FOLLOW_USER_REQUEST_CODE = 65,
        UNFOLLOW_USER_REQUEST_CODE = 66,
        GET_RECENT_POSTS_REQUEST_CODE = 67,
        GET_FOLLOWING_POSTS_REQUEST_CODE = 68,
        GET_ONLINE_USERS_REQUEST_CODE = 69,
        SEND_MESSAGE_REQUEST_CODE = 70,
        SEND_AES_SETUP_REQUEST_CODE = 71,
        GET_SERVER_RSA_PUBLIC_KEY_REQUEST_CODE = 72
    }
}
