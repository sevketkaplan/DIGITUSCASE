using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace WM.Extentions
{

    public class CheckAuth : ActionFilterAttribute
    {
        public string controller;
        public string action;
        private bool loginAccess = true;
        public int id;
        public string token;
        public CheckAuth(string controller = "", string action = "", bool loginAccess = true, int id = 0, string token = "")
        {
            this.controller = controller;
            this.action = action;
            this.loginAccess = loginAccess;
            this.id = id;
            this.token = token;
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string url = context.HttpContext.Request.GetDisplayUrl();
            this.action = context.ActionDescriptor.RouteValues["action"];
            this.controller = context.ActionDescriptor.RouteValues["controller"];


            if (string.IsNullOrEmpty(this.action))
            {
                this.action = context.ActionDescriptor.RouteValues["action"];
            }

            if (string.IsNullOrEmpty(this.controller))
            {
                this.controller = context.ActionDescriptor.RouteValues["controller"];
            }
            if (!string.IsNullOrEmpty(this.controller) && !string.IsNullOrEmpty(this.action))
            {
                if (controller == "Admin" && (action == "Login" || action == "Logout"))
                {
                    this.loginAccess = false;
                }
                if (loginAccess)
                {
                    string id;
                    DateTime date;
                    if (context != null)
                    {
                        if (context.HttpContext.Request.Cookies.Keys != null)
                        {
                            try
                            {
                                var querystring = context.HttpContext.Request.QueryString.ToString();

                                var vid = Encryption.Decrypt(context.HttpContext.Request.Cookies["vid"]);
                                string[] paramss = vid.Split("-");
                                id = paramss[0];
                                date = DateTime.Parse(paramss[1]).Date;

                                var token = context.HttpContext.Request.Cookies["usertoken"];

                                if (!Functions.IsNumeric(id) || date < DateTime.Now.Date || string.IsNullOrEmpty(token))
                                {
                                    context.Result = new RedirectResult("/si_panel?returnURL=" + controller + "/" + action + "?" + querystring);
                                    context.HttpContext.Response.Redirect("/si_panel?returnURL=" + controller + "/" + action + "?" + querystring);
                                }

                            }
                            catch (Exception e)
                            {
                                context.Result = new RedirectResult("/si_panel?returnURL=" + context.HttpContext.Request.GetDisplayUrl());
                                context.HttpContext.Response.Redirect("/si_panel?returnURL=" + context.HttpContext.Request.GetDisplayUrl());
                            }
                        }
                    }
                }
            }
            //}

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