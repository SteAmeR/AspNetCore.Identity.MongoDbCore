﻿using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoIdentitySample.Mvc.Models;
using MongoIdentitySample.Mvc.Services;
using Microsoft.Extensions.Logging;
using System;

namespace MongoIdentitySample.Mvc
{
    public class Startup
    {
        private IWebHostEnvironment _env;
        public Startup(IWebHostEnvironment env)
        {
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                //per user config that is not committed to repo, use this to override settings (e.g. connection string) based on your local environment.
                .AddJsonFile($"appsettings.local.json", optional: true); 

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            var mongoSettings = Configuration.GetSection(nameof(MongoDbSettings));
            var settings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

            services.AddSingleton<MongoDbSettings>(settings);

            services.AddIdentity<ApplicationUser, MongoIdentityRole>()
                    .AddMongoDbStores<ApplicationUser, MongoIdentityRole, Guid>(settings.ConnectionString, settings.DatabaseName)
                    .AddSignInManager()
                    .AddDefaultTokenProviders();


            var builder = services.AddRazorPages();
            
            #if DEBUG
                if(_env.IsDevelopment())
                {
                    builder.AddRazorRuntimeCompilation();
                }
            #endif

            services.AddMvc();

            services.AddApplicationInsightsTelemetry();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
