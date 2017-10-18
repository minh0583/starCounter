using System;
using Starcounter;
using Simplified.Ring2;
using Simplified.Ring3;

namespace StarCounter.App.Service.UserAdmin
{
    public class SystemUserAdmin
    {
        /// <summary>
        /// Assigns person to system user WhatIs property.
        /// </summary>
        /// <param name="systemUser"></param>
        /// <param name="firstName"></param>
        /// <param name="lastname"></param>
        public static void AssignPerson(SystemUser systemUser, string firstName, string lastname)
        {
            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentException(nameof(firstName));
            }

            if (string.IsNullOrEmpty(lastname))
            {
                throw new ArgumentException(nameof(lastname));
            }

            if (systemUser == null)
            {
                throw new ArgumentException(nameof(systemUser));
            }

            var person = Db.SQL<Person>("SELECT o FROM Simplified.Ring2.Person o WHERE o.FirstName=? AND o.LastName=?", firstName, lastname).First;
            if (person == null)
            {
                person = new Person { FirstName = firstName, LastName = lastname };
            }

            systemUser.WhatIs = person;
        }

        /// <summary>
        /// Delete System user
        /// </summary>
        /// <param name="user"></param>
        public static void DeleteSystemUser(SystemUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Remove Email adresses associated to the system user
            //            Db.SlowSQL("DELETE FROM Simplified.Ring3.EmailAddress WHERE ToWhat=?", user);

            // Remove ResetPassword associated to the system user Sombody
            Db.SlowSQL("DELETE FROM Simplified.Ring6.ResetPassword WHERE User=?", user);

            // TODO: Should we also delete the Somebody (Person/Company)?

            // Remove system user group member (If system user is member of a system user group)
            Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUser=?", user);

            user.Delete();
        }

        /// <summary>
        /// Delete System User Group and it's relationships
        /// </summary>
        /// <param name="group"></param>
        public static void DeleteSystemUserGroup(SystemUserGroup group)
        {
            // Remove System user member's
            Db.SlowSQL("DELETE FROM Simplified.Ring3.SystemUserGroupMember WHERE SystemUserGroup=?", group);
            group.Delete();
        }

        /// <summary>
        /// Add System User as a Member of a SystemUserGroup
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        public static void AddSystemUserToSystemUserGroup(SystemUser user, SystemUserGroup group)
        {

            SystemUserGroupMember systemUserGroupMember = new SystemUserGroupMember();
            systemUserGroupMember.WhatIs = user;
            systemUserGroupMember.ToWhat = group;
            //systemUserGroupMember.SetSystemUser(user);
            //systemUserGroupMember.SetToWhat(group);
            //group.AddMember(systemUser);
        }

        /// <summary>
        /// Remove System User as a Member of a SystemUserGroup
        /// </summary>
        /// <param name="user"></param>
        /// <param name="group"></param>
        public static void RemoveSystemUserFromSystemUserGroup(SystemUser user, SystemUserGroup group)
        {
            var removeGroup = Db.SQL<SystemUserGroupMember>("SELECT o FROM Simplified.Ring3.SystemUserGroupMember o WHERE o.WhatIs=? AND o.ToWhat=?", user, group).First;
            removeGroup?.Delete();

            //group.RemoveMember(user);
        }

        /// <summary>
        /// Set password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public static void SetPassword(SystemUser user, string password)
        {
            string salt = SystemUser.GeneratePasswordSalt(16);
            string passwordHash = SystemUser.GeneratePasswordHash(user.Username, password, salt);

            user.Password = passwordHash;
            user.PasswordSalt = salt;
        }
    }
}
