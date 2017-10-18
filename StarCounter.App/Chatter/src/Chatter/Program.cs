using System;
using Starcounter;
using Simplified.Ring6;

namespace StarCounter.App.Client.Chatter
{
    class Program
    {
        static void Main()
        {
            MainHandlers handlers = new MainHandlers();
            CommitHooks hooks = new CommitHooks();

            handlers.Register();
            hooks.Register();
        }
    }
}
