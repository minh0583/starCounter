using Starcounter;
using StarCounter.App.Client.Chatter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCounter.App.Client.Chatter.Api
{
    internal class PartialHandlers
    {
        public void Register()
        {
            Handle.GET("/chatter/partial/unauthorized?return_uri={?}", (string returnUri) =>
            {
                return new UnauthorizedPage();
            });
        }
    }
}
