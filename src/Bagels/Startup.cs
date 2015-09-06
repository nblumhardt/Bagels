using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Serilog;
using InfluxDB.LineProtocol;
using System;
using System.Collections.Generic;
using Bagels.Transport.InProc;
using Bagels.Transport;

namespace Bagels
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            Log.Information("Starting up");

            Metrics.Collector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromSeconds(5))
                .Tag.With("host", Environment.GetEnvironmentVariable("COMPUTERNAME"))
                .WriteTo.InfluxDB("http://192.168.99.100:8086", "data")
                .CreateCollector();

            var builder = new ConfigurationBuilder(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IMessageBus, InProcMessageBus>();

            foreach (var t in typeof(InProcMessageBus).Assembly.ExportedTypes)
            {
                if (t.BaseType != null && t.BaseType.IsConstructedGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(MessageHandler<>))
                    services.AddTransient(t.BaseType, t);
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Verbose;
            loggerFactory.AddSerilog();

            app.Use(async (context, next) =>
            {
                using (Metrics.Time("http_request", new Dictionary<string, string> {
                    { "method", context.Request.Method },
                    { "path", context.Request.Path }
                }))
                {
                    await next();
                }
            });

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseErrorPage();
            }
            else
            {
                app.UseErrorHandler("/Order/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Order}/{action=Index}/{id?}");
            });

            // Autofac's IStartable would be great here.
            ((InProcMessageBus)app.ApplicationServices.GetRequiredService<IMessageBus>()).Start();
        }
    }
}
