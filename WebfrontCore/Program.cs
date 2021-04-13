﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibraryCore.Interfaces;
using WebfrontCore.Middleware;

namespace WebfrontCore
{
    public class Program
    {
        public static IManager Manager;
        public static IServiceCollection Services;
        public static IServiceProvider ApplicationServiceProvider;

        public static Task Init(IManager mgr, IServiceProvider existingServiceProvider, IServiceCollection services, CancellationToken cancellationToken)
        {
            Services = services;
            Manager = mgr;
            ApplicationServiceProvider = existingServiceProvider;
            var config = Manager.GetApplicationSettings().Configuration();
            Manager.MiddlewareActionHandler.Register(null, new CustomCssAccentMiddlewareAction("#007ACC", "#fd7e14", config.WebfrontPrimaryColor, config.WebfrontSecondaryColor), "custom_css_accent");
            return BuildWebHost().RunAsync(cancellationToken);
        }

        private static IWebHost BuildWebHost()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            return new WebHostBuilder()
#if DEBUG
                .UseContentRoot(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\", "WebfrontCore")))
#else
                .UseContentRoot(SharedLibraryCore.Utilities.OperatingDirectory)
#endif
                .UseUrls(Manager.GetApplicationSettings().Configuration().WebfrontBindUrl)
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();
        }
    }
}
