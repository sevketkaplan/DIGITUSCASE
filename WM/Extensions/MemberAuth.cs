using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using System;


namespace WM.Extentions
{
    public class MemberAuth : ActionFilterAttribute
    {
        public string controller;
        public string action;
        private bool loginAccess = true;

        public MemberAuth(string controller = "", string action = "", bool loginAccess = true)
        {
            this.controller = controller;
            this.action = action;
            this.loginAccess = loginAccess;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (this.action == "")
            {
                this.action = context.ActionDescriptor.RouteValues["action"];
            }

            if (this.controller == "")
            {
                this.controller = context.ActionDescriptor.RouteValues["controller"];
            }
            if (this.controller != "" && this.action != "")
            {
                if (controller == "Admin" && (action == "Login" || action == "Logout"))
                {
                    this.loginAccess = false;
                }

                if (loginAccess)
                {
                    string cookieUsername;
                    string id;
                    string tarih;
                    if (context != null)
                    {
                        if (context.HttpContext.Request.Cookies.Keys != null)
                        {
                            try
                            {
                                cookieUsername = context.HttpContext.Request.Cookies["musername"];
                                var vid = Encryption.Decrypt(context.HttpContext.Request.Cookies["mid"]);
                                string[] asd = vid.Split("-");
                                id = asd[0];
                                tarih = DateTime.Parse(asd[1]).ToString("d");
                                if (tarih != DateTime.UtcNow.ToString("d") || !Functions.IsNumeric(id) || cookieUsername == "")
                                {
                                    var querystring = context.HttpContext.Request.QueryString.ToString();
                                    context.HttpContext.Response.Redirect("/UyeFirmalar/Giris?returnURL=" + controller + "/" + action + "?" + querystring);
                                }
                            }
                            catch (Exception e)
                            {
                                context.HttpContext.Response.Redirect("/UyeFirmalar/Giris?returnURL=" + context.HttpContext.Request.GetDisplayUrl());
                            }
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {

            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {

            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var c = context;
            base.OnResultExecuted(context);
        }
    }

}
