﻿using Simplified.Ring6;

namespace StarCounter.App.Service.Chatter.Helpers
{
    public class ChatMessageTextValidator
    {
        public static string IsValid(ChatMessageText chatMessageText)
        {
            return string.IsNullOrEmpty(chatMessageText.Text) ? "Message cannot be empty!" : string.Empty;
        }
    }
}
