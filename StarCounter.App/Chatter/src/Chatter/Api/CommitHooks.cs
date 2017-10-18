using StarCounter.App.Client.Chatter.Helpers;
using Starcounter;
using Simplified.Ring5;

namespace StarCounter.App.Client.Chatter
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
