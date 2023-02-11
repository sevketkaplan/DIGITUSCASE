
using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Core.ResultType;
using DataAccessLayer.WMDbContext;
using Interfaces.Business;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace WM.Extentions
{

    public class ApiCheckAuth : ActionFilterAttribute
    {

        public string controller;
        public string action;
        private bool loginAccess = true;
        public int id;
        public string token;
        public ApiCheckAuth(string token = "", string controller = "", string action = "", bool loginAccess = true, int id = 0)
        {
            this.token = token;
            this.controller = controller;
            this.action = action;
            this.loginAccess = loginAccess;
            this.id = id;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string url = context.HttpContext.Request.GetDisplayUrl();
            this.action = context.ActionDescriptor.RouteValues["action"];
            this.controller = context.ActionDescriptor.RouteValues["controller"];

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
                string token;
                if (context != null)
                {
                    if (context.HttpContext.Request.Query.Keys != null)
                    {
                        try
                        {
                            token = context.HttpContext.Request.Query["token"];

                            try
                            {

                                using (WMDbContext db = new WMDbContext())
                                {
                                    var user = db.Admin.Where(s => s.Token == token);
                                    if (user != null)
                                    {
                                        context.Result = new RedirectResult("/ApiExceptionResult/State/");
                                        context.HttpContext.Response.Redirect("/ApiExceptionResult/State/");

                                    }
                                    else
                                    {
                                        context.HttpContext.Response.Redirect("/Admin/Check");
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                context.HttpContext.Response.Redirect("/Admin/Check");

                            }

                        }
                        catch (Exception e)
                        {

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