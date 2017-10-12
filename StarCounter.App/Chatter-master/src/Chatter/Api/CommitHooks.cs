using Starcounter;
using Simplified.Ring5;
using StarCounter.App.Service.Chatter.Helpers;

namespace StarCounter.App.Service.Chatter
{
    internal class CommitHooks
    {
        public void Register()
        {
            Hook<SystemUserSession>.CommitInsert += (s, a) =>
            {
                PageManager.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitDelete += (s, a) =>
            {
                PageManager.RefreshSignInState();
            };

            Hook<SystemUserSession>.CommitUpdate += (s, a) =>
            {
                PageManager.RefreshSignInState();
            };
        }
    }
}
