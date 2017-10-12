﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Starcounter;
using Simplified.Ring3;
using Simplified.Ring5;
using Simplified.Ring2;
using Simplified.Ring6;

namespace StarCounter.App.Service.SignIn
{
    internal class MainHandlers
    {
        protected string AuthCookieName = "soauthtoken";
        protected int rememberMeDays = 30;

        public void Register()
        {
            Application.Current.Use(new HtmlFromJsonProvider());
            Application.Current.Use(new PartialToStandaloneHtmlProvider());

            //Testing JWT
            /*Handle.GET("/signin/jwt/{?}/{?}", (string Username, string Password) => {
                string message;
                SystemUserSession session = SignInOut.SignInSystemUser(Username, Password, null, out message);

                if (session != null) {
                    string jwt = JWT.JsonWebToken.Encode(new { Username = Username, Issuer = "Polyjuice.SignIn" }, session.Token.User.Password, JWT.JwtHashAlgorithm.HS256);
                    Handle.AddOutgoingHeader("x-jwt", jwt);
                }

                return 200;
            });*/

            Application.Current.Use((Request req) =>
            {
                Cookie cookie = GetSignInCookie();

                if (cookie != null)
                {
                    if (Session.Current == null)
                    {
                        Session.Current = new Session(SessionOptions.PatchVersioning);
                    }

                    SystemUserSession session = SystemUser.SignInSystemUser(cookie.Value);

                    if (session != null)
                    {
                        RefreshAuthCookie(session);
                    }
                }

                return null;
            });

            Handle.GET("/signin/user", () =>
            {
                MasterPage master = this.GetMaster();

                if (master.SignInPage != null)
                {
                    return master.SignInPage;
                }

                Cookie cookie = GetSignInCookie();
                SignInPage page = new SignInPage() { Data = null };

                master.SignInPage = page;

                if (cookie != null)
                {
                    SystemUser.SignInSystemUser(cookie.Value);
                    master.RefreshSignInState();
                }

                //Testing JWT
                /*if (Handle.IncomingRequest.HeadersDictionary.ContainsKey("x-jwt")) {
                    System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string jwt = Handle.IncomingRequest.HeadersDictionary["x-jwt"];
                    Dictionary<string, string> payload = JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, string.Empty, false);
                    string username = payload["Username"];
                    SystemUser user = Db.SQL<SystemUser>("SELECT su FROM Simplified.Ring3.SystemUser su WHERE su.Username = ?", username).First;

                    try {
                        JWT.JsonWebToken.DecodeToObject<Dictionary<string, string>>(jwt, user.Password, true);
                        page.SetAuthorizedState(SignInOut.SignInSystemUser(user));
                    } catch (JWT.SignatureVerificationException) { 
                    }
                }*/

                return page;
            });

            Handle.GET("/signin/partial/signout", HandleSignOut, new HandlerOptions() { SkipRequestFilters = true });

            Handle.GET("/signin/signinuser", HandleSignInForm);
            Handle.GET<string>("/signin/signinuser?{?}", HandleSignInForm);

            Handle.GET("/signin/profile", () =>
            {
                MasterPage master = this.GetMaster();

                master.RequireSignIn = true;
                master.Open("/signin/partial/profile-form");

                return master;
            });

            Handle.GET("/signin/partial/signin-form", () => new SignInFormPage() { Data = null }, new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/alreadyin-form", () => new AlreadyInPage() { Data = null },
                new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/restore-form", () => new RestorePasswordFormPage(),
                new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/profile-form", () => new ProfileFormPage() { Data = null },
                new HandlerOptions() { SelfOnly = true });
            Handle.GET("/signin/partial/accessdenied-form", () => new AccessDeniedPage(),
                new HandlerOptions() { SelfOnly = true });

            Handle.GET("/signin/partial/main-form", () => new MainFormPage() { Data = null },
                new HandlerOptions() { SelfOnly = true });

            Handle.GET("/signin/partial/user/image", () => new UserImagePage());
            Handle.GET("/signin/partial/user/image/{?}", (string objectId) => new Json(),
                new HandlerOptions { SelfOnly = true });

            Handle.GET("/signin/generateadminuser", (Request request) =>
            {
                if (Db.SQL("SELECT o FROM Simplified.Ring3.SystemUser o").First != null)
                {
                    Handle.SetOutgoingStatusCode(403);
                    return "Unable to generate admin user: database is not empty!";
                }

                string ip = request.ClientIpAddress.ToString();

                if (ip == "127.0.0.1" || ip == "localhost")
                {
                    SignInOut.AssureAdminSystemUser();

                    return "Default admin user has been successfully generated.";
                }

                Handle.SetOutgoingStatusCode(403);
                return "Access denied.";

            }, new HandlerOptions() { SkipRequestFilters = true });

            Handle.POST("/signin/partial/signin", (Request request) =>
            {
                NameValueCollection values = HttpUtility.ParseQueryString(request.Body);
                string username = values["username"];
                string password = values["password"];
                string rememberMe = values["rememberMe"];

                HandleSignIn(username, password, rememberMe);
                Session.Current.CalculatePatchAndPushOnWebSocket();

                return 200;
            }, new HandlerOptions() { SkipRequestFilters = true });

            Handle.GET("/signin/admin/settings", (Request request) =>
            {
                Json page;
                if (!AuthorizationHelper.TryNavigateTo("/signin/admin/settings", request, out page))
                {
                    return page;
                }

                return Db.Scope(() =>
                {
                    var settingsPage = new SettingsPage
                    {
                        Html = "/SignIn/viewmodels/SettingsPage.html",
                        Uri = request.Uri,
                        Data = MailSettingsHelper.GetSettings()
                    };
                    return settingsPage;
                });
            });

            // Reset password
            Handle.GET("/signin/user/resetpassword?{?}", (string query, Request request) =>
            {
                NameValueCollection queryCollection = HttpUtility.ParseQueryString(query);
                string token = queryCollection.Get("token");

                MasterPage master = this.GetMaster();

                if (token == null)
                {
                    // TODO:
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                // Retrive the resetPassword instance
                ResetPassword resetPassword = Db.SQL<ResetPassword>("SELECT o FROM Simplified.Ring6.ResetPassword o WHERE o.Token=? AND o.Expire>?", token, DateTime.UtcNow).First;

                if (resetPassword == null)
                {
                    // TODO: Show message "Reset token already used or expired"
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                if (resetPassword.User == null)
                {
                    // TODO: Show message "User deleted"
                    master.Partial = null; // (ushort)System.Net.HttpStatusCode.NotFound;
                    return master;
                }

                SystemUser systemUser = resetPassword.User;

                ResetPasswordPage page = new ResetPasswordPage()
                {
                    Html = "/SignIn/viewmodels/ResetPasswordPage.html",
                    Uri = "/signin/user/resetpassword"
                    //Uri = request.Uri // TODO:
                };

                page.ResetPassword = resetPassword;

                if (systemUser.WhoIs != null)
                {
                    page.FullName = systemUser.WhoIs.FullName;
                }
                else
                {
                    page.FullName = systemUser.Username;
                }

                master.Partial = page;

                return master;
            });

            Handle.GET("/signin/user/authentication/settings/{?}", (string userid, Request request) =>
            {
                Json page;
                if (!AuthorizationHelper.TryNavigateTo("/signin/user/authentication/settings/{?}", request, out page))
                {
                    return new Json();
                }

                // Get system user
                SystemUser user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID = ?", userid).FirstOrDefault();

                if (user == null)
                {
                    // TODO: Return a "User not found" page
                    return new Json();
                    //return (ushort)System.Net.HttpStatusCode.NotFound;
                }

                SystemUser systemUser = SystemUser.GetCurrentSystemUser();
                SystemUserGroup adminGroup = Db.SQL<SystemUserGroup>("SELECT o FROM Simplified.Ring3.SystemUserGroup o WHERE o.Name = ?",
                        AuthorizationHelper.AdminGroupName).FirstOrDefault();

                // Check if current user has permission to get this user instance
                if (AuthorizationHelper.IsMemberOfGroup(systemUser, adminGroup))
                {
                    if (user.WhoIs is Person)
                    {
                        page = Db.Scope(() => new SystemUserAuthenticationSettings
                        {
                            Html = "/SignIn/viewmodels/SystemUserAuthenticationSettings.html",
                            Uri = request.Uri,
                            Data = user,
                            UserPassword = Self.GET("/signin/user/authentication/password/" + user.GetObjectID())
                        });

                        return page;
                    }
                }

                return new Json();
            }, new HandlerOptions { SelfOnly = true });

            Handle.GET("/signin/user/authentication/password/{?}", (string userid, Request request) =>
            {
                // Get system user
                SystemUser user = Db.SQL<SystemUser>("SELECT o FROM Simplified.Ring3.SystemUser o WHERE o.ObjectID = ?", userid).FirstOrDefault();

                if (user == null)
                {
                    return new Json();
                }

                Json page = Db.Scope(() => new SetPasswordPage
                {
                    Html = "/SignIn/viewmodels/SetPasswordPage.html",
                    Data = user
                });

                return page;
            }, new HandlerOptions { SelfOnly = true });

            Blender.MapUri("/signin/user", "user"); //expandable icon; used in Launcher
            Blender.MapUri("/signin/signinuser", "userform"); //inline form; used in RSE Launcher
            Blender.MapUri("/signin/signinuser?{?}", "userform-return"); //inline form; used in UserAdmin
            Blender.MapUri("/signin/admin/settings", "settings");
            Blender.MapUri("/signin/user/authentication/password/{?}", "authentication-password");
            Blender.MapUri("/signin/user/authentication/settings/{?}", "authentication-settings");
            Blender.MapUri("/signin/partial/user/image", "userimage-default");  // default user image
        }

        protected void ClearAuthCookie()
        {
            this.SetAuthCookie(null);
        }

        protected void RefreshAuthCookie(SystemUserSession Session)
        {
            Cookie cookie = GetSignInCookie();

            if (cookie == null)
            {
                return;
            }

            Db.Transact(() =>
            {
                Session.Token = SystemUser.RenewAuthToken(Session.Token);
                if (Session.Token.IsPersistent)
                {
                    Session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                }
            });

            cookie.Value = Session.Token.Token;
            if (Session.Token.IsPersistent)
            {
                cookie.Expires = Session.Token.Expires;
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected void SetAuthCookie(SystemUserTokenKey token)
        {
            Cookie cookie = new Cookie()
            {
                Name = AuthCookieName
            };

            if (token == null)
            {
                //to delete a cookie, explicitly use a date in the past
                cookie.Expires = DateTime.Now.AddDays(-1).ToUniversalTime();
            }
            else
            {
                cookie.Value = token.Token;
                if (token.IsPersistent)
                {
                    cookie.Expires = token.Expires;
                }
            }

            Handle.AddOutgoingCookie(cookie.Name, cookie.GetFullValueString());
        }

        protected MasterPage GetMaster()
        {
            Session session = Session.Current;

            //if (session != null && session.Data != null)
            //{
            //    return session.Data as MasterPage;
            //}

            if (session != null && session.Store != null)
            {
                return session.Store as MasterPage;
            }

            MasterPage master = new MasterPage();

            if (session == null)
            {
                session = new Session(SessionOptions.PatchVersioning);
            }

            //master.Session = session;
            master.Data = session;
            return master;
        }

        protected void HandleSignIn(string Username, string Password, string RememberMe)
        {
            Username = Uri.UnescapeDataString(Username);

            SystemUserSession session = SystemUser.SignInSystemUser(Username, Password);

            if (session == null)
            {
                MasterPage master = GetMaster();
                string message = "Invalid username or password!";

                if (master.SignInPage != null)
                {
                    master.SignInPage.Message = message;
                }

                if (master.Partial is MainFormPage)
                {
                    MainFormPage page = (MainFormPage)master.Partial;
                    if (page.CurrentForm is SignInFormPage)
                    {
                        SignInFormPage form = (SignInFormPage)page.CurrentForm;
                        form.Message = message;
                    }
                }

                if (master.Partial is SignInFormPage)
                {
                    SignInFormPage page = master.Partial as SignInFormPage;
                    page.Message = message;
                }
            }
            else
            {
                if (RememberMe == "true")
                {
                    Db.Transact(() =>
                    {
                        session.Token.Expires = DateTime.UtcNow.AddDays(rememberMeDays);
                        session.Token.IsPersistent = true;
                    });
                }
                SetAuthCookie(session.Token);
            }
        }

        protected Response HandleSignInForm()
        {
            return this.HandleSignInForm(string.Empty);
        }

        protected Response HandleSignInForm(string OriginalUrl)
        {
            MasterPage master = this.GetMaster();

            master.RequireSignIn = false;
            master.OriginalUrl = HttpUtility.UrlDecode(OriginalUrl);
            master.Open("/signin/partial/main-form");

            return master;
        }

        protected Response HandleSignOut()
        {
            SystemUser.SignOutSystemUser();
            ClearAuthCookie();

            return this.GetMaster();
        }

        protected Cookie GetSignInCookie()
        {
            List<Cookie> cookies = Handle.IncomingRequest.Cookies.Where(val => !string.IsNullOrEmpty(val)).Select(x => new Cookie(x)).ToList();
            Cookie cookie = cookies.FirstOrDefault(x => x.Name == AuthCookieName);

            return cookie;
        }
    }
}