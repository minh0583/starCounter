using Simplified.Ring1;
using Starcounter;
using Simplified.Ring6;
using Simplified.Ring5;
using Simplified.Ring3;
using System;
using System.Collections;

namespace StarCounter.App.Client.Chatter
{
    partial class LobbyPage : Json
    {
        public void RefreshData()
        {
            RefreshUser();
            CreateOneToOneGroup();
            GetTotalMessagesSent();
            ChatGroups = Db.SQL<ChatGroup>(@"SELECT g FROM Simplified.Ring6.ChatGroup g ORDER BY g.Name");                        
        }

        private void GetTotalMessagesSent()
        {
            TotalMessagesSent = Db.SlowSQL<long>("SELECT COUNT(*) FROM Simplified.Ring6.ChatMessage m WHERE m.UserName = ?", UserName).First;
        }

        private void CreateOneToOneGroup()
        {
            var users = Db.SQL<SystemUser>("SELECT u FROM Simplified.Ring3.SystemUser u");

            foreach(var user in users)
            {
                if (UserName == "Anonymous" || UserName == user.Name)
                    continue;

                CreateGroup(user.Name);
            }
        }

        private void CreateGroup(string name)
        {
            string groupName = string.Format("{0}-{1}", UserName, name);
            string groupNameResever = string.Format("{0}-{1}", name, UserName);
            var chatGroup = Db.SQL<ChatGroup>(@"SELECT g FROM Simplified.Ring6.ChatGroup g WHERE g.Name = ? OR g.Name = ?", groupName, groupNameResever).First;
            //var chatGroup = Db.SQL<ChatGroup>(@"SELECT g FROM Simplified.Ring6.ChatGroup g WHERE g.Name LIKE ?", "%" + UserName + "%").First;

            if (chatGroup == null)
            {
                ChatGroup group = null;

                Db.Transact(() =>
                {
                    group = new ChatGroup
                    {
                        Name = groupName
                    };
                });
            }
        }

        public void RefreshUser()
        {
            var session = GetCurrentSystemUserSession();

            if (session != null)
            {
                var user = session.Token.User;

                UserKey = user.Key;
                UserName = user.Username;
            }
            else
            {
                UserKey = null;
                UserName = "Anonymous";
            }
        }

        protected SystemUserSession GetCurrentSystemUserSession()
        {
            return Db.SQL<SystemUserSession>("SELECT o FROM Simplified.Ring5.SystemUserSession o WHERE o.SessionIdString = ?", Session.Current.SessionId).First;
        }

        void Handle(Input.GoToNewGroup Action)
        {
            var name = string.IsNullOrEmpty(this.NewGroupName) ? "Anonymous group" : this.NewGroupName;
            ChatGroup group = null;

            Db.Transact(() =>
            {
                group = new ChatGroup
                {
                    Name = name
                };
            });

            RedirectUrl = "/chatter/chatgroup/" + group.Key;
        }

        [LobbyPage_json.ChatGroups]
        partial class LobbyPageChatGroupRow : Json, IBound<ChatGroup>
        {
            void Handle(Input.Delete Action)
            {
                Db.Transact(() =>
                {
                    var messages = Db.SQL<ChatMessage>("SELECT m FROM Simplified.Ring6.ChatMessage m WHERE m.\"Group\" = ?", Data);

                    foreach (ChatMessage message in messages)
                    {
                        var relations = Db.SQL<Relation>("SELECT m FROM Simplified.Ring1.Relation m WHERE m.ToWhat = ?", message);
                        foreach (Relation relation in relations)
                        {
                            relation.WhatIs?.Delete();
                            relation.Delete();
                        }
                        message.Delete();
                    }

                    Data.Delete();
                });

                ParentPage.RefreshData();
            }

            public LobbyPage ParentPage
            {
                get
                {
                    return Parent.Parent as LobbyPage;
                }
            }

            protected override void OnData()
            {
                base.OnData();
                var notificationCount = GetNotificationCount();

                IsNotification = notificationCount > 0;
                NotificationCount = notificationCount.ToString();

                TotalMessages = GetTotalMessagesPerContact();

                Url = $"/chatter/chatgroup/{Key}";                
            }

            private long GetTotalMessagesPerContact()
            {
                return Db.SlowSQL<long>("SELECT count(*) FROM Simplified.Ring6.ChatGroup g INNER JOIN Simplified.Ring6.ChatMessage m ON g = m.\"Group\" where m.\"Group\" = ? ", Data).First;
            }

            private long GetNotificationCount()
            {
                DateTime lastUsed = DateTime.Now;

                var systemUser = Db.SQL<SystemUserTokenKey>("SELECT o FROM Simplified.Ring5.SystemUserTokenKey o WHERE CAST(o.User AS Simplified.Ring3.SystemUser).UserName = ? ORDER BY LastUsed DESC FETCH ? OFFSET ?", ParentPage.UserName, 1, 2).First;

                if (systemUser != null)
                {
                    lastUsed = systemUser.LastUsed.ToLocalTime();
                }                                

                return Db.SlowSQL<long>("SELECT count(g.Name) FROM Simplified.Ring6.ChatGroup g INNER JOIN Simplified.Ring6.ChatMessage m ON g = m.\"Group\" where m.\"Group\" = ? AND (g.Name LIKE '%" + ParentPage.UserName + "%'" + " OR g.Name not LIKE '%-%') AND m.UserName <> ? AND m.\"Date\" > ? GROUP BY g.Name",
                    Data, ParentPage.UserName, lastUsed).First;                
            }           
        }       
    }
}