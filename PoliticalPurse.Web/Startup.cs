using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PoliticalPurse.Web.Services;
using Newtonsoft.Json.Converters;
using PoliticalPurse.Web.Filters;

namespace PoliticalPurse.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                builder = builder.AddUserSecrets<Startup>();
            }

            builder = builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = new PathString("/user/sign_in");
                    o.LogoutPath = new PathString("/user/sign_out");
                });

            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(LoginContextFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter {
                    CamelCaseText = true
                });
            });

            services.AddOptions();
            services.Configure<DatabaseOptions>(Configuration);

            services.AddSingleton<UserService>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<DonationService>();
            services.AddSingleton<DonationUpdateService>();
            services.AddSingleton<PartyService>();

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();

            var supportedCultures = new[]
            {
                new CultureInfo("en-NZ"),
                new CultureInfo("en-US")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-NZ"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseStaticFiles();

            app.UseMvc();

            app.Run(async (context) =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not found");
            });
        }
    }
}
