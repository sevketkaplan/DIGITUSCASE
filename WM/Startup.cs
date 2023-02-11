using BusinessLogicLayer;
using DataObject.Helpers;
using DinkToPdf;
using DinkToPdf.Contracts;
using Interfaces.Business;
using Interfaces.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Pioneer.Pagination;
using Services;
using StructureMap;
using System;
using System.Globalization;
using System.Text;
using Utility.Security.Jwt;
using WM.Extensions;

//[assembly: OwinStartupAttribute(typeof(WM.Startup))]
namespace WM
{
    public partial class Startup
    {
        //ILogger<LogInterceptor> _logger;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            services.AddCors();
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("tr"),
                    new CultureInfo("de"),
                    new CultureInfo("ru"),
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "tr", uiCulture: "tr");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            MapperBL.Initialize();
            services.AddSingleton<IAdminService, AdminService>();
            services.AddSingleton<IPaginatedMetaService, PaginatedMetaService>();
            



            services.AddSignalR();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services
                .AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.Culture = CultureInfo.CurrentCulture);

            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        options.LoginPath = "/Account/Login/";
            //    });
            //services.AddAuthentication()
            //    .AddCookie("vusername", o => // scheme1
            //    {
            //        o.ExpireTimeSpan = TimeSpan.FromHours(1);
            //        o.LoginPath = new Microsoft.AspNetCore.Http.PathString("/si_panel");
            //        o.Cookie.Name = "vusername";
            //        o.SlidingExpiration = true;
            //    })
            //    .AddCookie("vid", o => //scheme2
            //    {
            //        o.ExpireTimeSpan = TimeSpan.FromHours(1);
            //        o.LoginPath = new Microsoft.AspNetCore.Http.PathString("/si_panel");
            //        o.Cookie.Name = "vid";
            //        o.SlidingExpiration = true;
            //    });

            services.AddResponseCaching(_ =>
            {
                _.MaximumBodySize = 250;
                _.SizeLimit = 250;
                _.UseCaseSensitivePaths = false;
            });

            var container = ContainerConfigurator.Configure(services);
            return container.GetInstance<IServiceProvider>();
        }
        //public static class TokenLifetimeValidator
        //{
        //    public static bool Validate(
        //        DateTime? notBefore,
        //        DateTime? expires,
        //        SecurityToken tokenToValidate,
        //        TokenValidationParameters @param
        //    )
        //    {
        //        return (expires != null && expires > DateTime.Now);
        //    }
        //}
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection(); // UseHttpsRedirection en sondaki middleware olamaz Map Route öncesi olması gerekiyor.
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            //app.UseRequestLocalization(new RequestLocalizationOptions
            //{
            //    DefaultRequestCulture = new RequestCulture("tr-TR"),
            //});
            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("tr-TR");
            //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("tr-TR");

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseResponseCaching();


            app.UseSignalR(routes =>
            {
                routes.MapHub<WMHub>("/chaufferLocationHub");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Admin}/{action=Index}/{id?}");

                routes.MapRoute(
                 name: "si_panel",
                 template: "/si_panel",
                 defaults: new { controller = "Admin", action = "Login" });
            });


        }


    }
    public class ContainerConfigurator
    {
        public static Container Configure(IServiceCollection services)
        {
            var container = new Container();
            container.Configure(config =>
            {
                Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                config.For<IAdminBL>().Use<AdminBL>().Singleton();
                //tokenBL
                config.For<ITokenHelper>().Use<JwtHelper>().Singleton();
                config.Populate(services);

            });

            return container;
        }
    }

}


