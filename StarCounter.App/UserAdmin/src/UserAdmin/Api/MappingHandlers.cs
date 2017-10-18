using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;

namespace StarCounter.App.Service.UserAdmin.Api
{
    class MappingHandlers
    {
        public static void Register()
        {
            Blender.MapUri("/useradmin/menu", string.Empty, new string[] { "menu" });
            Blender.MapUri("/useradmin/app-name", string.Empty, new string[] { "app", "icon" });
            Blender.MapUri("/useradmin/search/{?}", string.Empty, new string[] { "search" });
            Blender.MapUri("/useradmin/partial/unauthenticated?return_uri={?}", string.Empty, new string[] { "redirection" });
            Blender.MapUri("/useradmin/user/authentication/password/{?}", string.Empty, new string[] { "authentication-password" });
            Blender.MapUri("/useradmin/user/authentication/settings/{?}", string.Empty, new string[] { "authentication-settings" });
            // Mapping issue
            // https://github.com/Starcounter/Starcounter/issues/2902
            Blender.MapUri<Person>("/UserAdmin/persons/{?}",
                paramsFrom =>
                {
                    var objectId = paramsFrom[0];
                    var user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID=?", objectId).First;
                    if (user != null)
                    {
                        paramsFrom[0] = user.WhatIs.Key;
                    }

                    return null;
                },
                paramsTo =>
                {
                    var objectId = paramsTo[0];
                    var user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.WhatIs.ObjectID=?", objectId).First;

                    if (user != null)
                    {
                        paramsTo[0] = user.Key;
                        return paramsTo;
                    }
                    return null;
                }
            , new string[] { "page" });
        }
    }
}
