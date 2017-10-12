using Simplified.Ring6;
using Starcounter;
using StarCounter.App.Service.Chatter.Helpers;

namespace StarCounter.App.Service.Chatter
{
    partial class ChatMessageTextWarningPage : Json
    {
        public void RefreshData(ChatMessageTextRelation textRelation)
        {
            Warning = ChatMessageTextValidator.IsValid(textRelation.Content);
            var relation = Db.SQL<ChatWarning>(@"Select m from Simplified.Ring6.ChatWarning m Where m.ErrorRelation = ?", textRelation).First;
            if (!string.IsNullOrEmpty(Warning))
            {
                if (relation == null)
                {
                    new ChatWarning
                    {
                        ErrorRelation = textRelation
                    };
                }
            }
            else
            {
                relation?.Delete();
            }
        }
    }
}
