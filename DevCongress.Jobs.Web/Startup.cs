using Autofac;
using Enexure.MicroBus;
using Enexure.MicroBus.Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plutonium.Reactor.Options;
using Serilog;
using static DevCongress.Jobs.Core.Entry;
using static Plutonium.Reactor.Entry;

namespace DevCongress.Jobs.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .Configure<ConnectionString>(Configuration.GetSection("ConnectionString"))
                .Configure<Messaging>(Configuration.GetSection("Messaging"))
            ;

            #region form upload limit

            //// uncomment to increase form upload limits
            //services.Configure<FormOptions>(x =>
            //{
            //  x.ValueLengthLimit = int.MaxValue;
            //  x.MultipartBodyLengthLimit = int.MaxValue;
            //  x.MultipartHeadersLengthLimit = int.MaxValue;
            //  x.KeyLengthLimit = int.MaxValue;

            //  x.ValueCountLimit = int.MaxValue;
            //});

            #endregion form upload limit

            services.AddNodeServices();
            services.AddMemoryCache();

            // Add framework services.
            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(o =>
            {
                // {2} is area, {1} is controller,{0} is the action
                o.ViewLocationFormats.Add("/Views/.pt/{1}/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/.pt/Shared/{0}" + RazorViewEngine.ViewExtension);
            });

            services.AddCore();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                     .ActionContext;
                return new UrlHelper(actionContext);
            });
        }

        // This method gets called by the runtime. Use this method to add services to the autofac container.
        public void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterModule(new ReactorModule()
                    .UseHandleBarsRenderer()
                    .UseNodePDFGenerator()
                    .UseMemoryCache()
                //.UseSendGrid()
                )
                .RegisterModule(new CoreModule())
            ;

            var busBuilder
                = new BusBuilder()
                    .RegisterReactor(HostAuthType.Asp)
                    .RegisterHandlers(typeof(Program).Assembly)
                    .RegisterCore()
                ;

            containerBuilder
                .RegisterMicroBus(busBuilder)
                .RegisterAssemblyTypes(typeof(Program).Assembly)
                   .AsImplementedInterfaces();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory
                    .AddConsole()
                    .AddDebug()
                    .AddSerilog();

            if (env.IsDevelopment())
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app
                .UseStatusCodePages()
                .UseStaticFiles()
                .UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

            #region cors

            //// configure cors
            //app.UseCors(builder =>
            //   builder
            //       .AllowAnyOrigin()
            //       .AllowAnyHeader()
            //       .AllowAnyMethod()
            //       .AllowCredentials()
            //   );

            #endregion cors

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            })
            ;
        }
    }
}