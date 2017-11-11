using Simplified.Ring6;
using Starcounter;

namespace StarCounter.App.Client.Chatter
{
    partial class ChatMessageTextPreviewPage : Json, IBound<ChatMessageText>
    {
        public void RefreshData(string chatMessageDraftId)
        {
            var chatMessageText = DbHelper.FromID(DbHelper.Base64DecodeObjectID(chatMessageDraftId)) as ChatMessageText;

            IsDeleted = chatMessageText.Deleted;

            if (IsDeleted)
            {                
                chatMessageText.Text = "This message has been removed.";
            }

            Data = chatMessageText;
            
        }

        void Handle(Input.Delete Action)
        {            
            Db.Transact(() =>
            {
                ChatMessageText message = Db.SQL<ChatMessageText>(@"
                SELECT m FROM Simplified.Ring6.ChatMessageText m WHERE m.Key = ?", Data.Key).First;

                if (message != null)
                    message.Deleted = true;
            });
        }
    }
}
