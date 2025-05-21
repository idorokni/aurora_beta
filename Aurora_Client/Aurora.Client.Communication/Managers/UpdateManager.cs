using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Client.Communication.Managers
{
    public class UpdateManager(object _currentViewModel)
    {
        public async Task HandleClientUpdate()
        {
            while (true)
            {
                //var info = await Communicator.Instance.ReadMessageFromServer(Communicator.Instance.ServerConnect);
            }
        }
    }
}
