using Aurora.Client.Communication.DataStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Services
{
    public class UpdateService
    {
        /*
        public static async Task UpdateViewModelWithChanges(object currentViewModel, RequestInfo info)
        {
            switch (info.code)
            {
                case RequestCode.GET_POST_DATA_REQUEST_CODE:
                    await HandlePostDataRequest(currentViewModel, info);
                    break;
                case RequestCode.GET_AMOUNT_OF_POSTS_REQUEST_CODE:
                    await HandleAmountOfPostsRequest(currentViewModel, info);
                    break;
                case RequestCode.PORT_SEND_REQUEST_CODE:
                    await HandlePortSendRequest(currentViewModel, info);
                    break;
                default:
                    break;
            }
        }
        */

        private static async Task HandlePostDataRequest(object currentViewModel, RequestInfo info)
        {
            // Handle post data request
        }
    }
}
