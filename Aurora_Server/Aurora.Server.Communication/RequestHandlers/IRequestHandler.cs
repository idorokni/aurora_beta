using Aurora.Server.Communication.DataStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Server.Communication.RequestHandlers
{
    public interface IRequestHandler
    {
        public bool IsRequestValid(RequestInfo info);
        public Task<(IRequestHandler, ResponseInfo)> HandleRequest(RequestInfo info);
    }
}
