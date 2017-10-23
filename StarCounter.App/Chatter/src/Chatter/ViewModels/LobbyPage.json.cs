using Simplified.Ring1;
using Starcounter;
using Simplified.Ring6;
using Simplified.Ring5;
using Simplified.Ring3;
using System;

namespace StarCounter.App.Client.Chatter
{
    partial class LobbyPage : Json
    {
        public void RefreshData()
        {
            RefreshUser();
            CreateOneToOneGroup();
            ChatGroups = Db.SQL<ChatGroup>(@"SELECT g FROM Simplified.Ring6.ChatGroup g ORDER BY g.Name");
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
            var chatGroup = Db.SQL<ChatGroup>(@"SELECT g FROM Simplified.Ring6.ChatGroup g WHERE g.Name = ?", groupName).First;

            if(chatGroup == null)
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
                Url = $"/chatter/chatgroup/{Key}";
            }
        }       
    }
}