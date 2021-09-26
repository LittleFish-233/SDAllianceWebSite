﻿using Blazored.LocalStorage;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SDAllianceWebSite.Shared.Application.Examines;
using SDAllianceWebSite.Shared.Application.Helper;
using SDAllianceWebSite.Shared.Application.Roles;
using SDAllianceWebSite.Shared.Component.Others.Images;
using SDAllianceWebSite.Shared.Provider;
using SDAllianceWebSite.Shared.Service;
using System.Net.Http;
using System.Text;

namespace SDAllianceWebSite.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddBootstrapBlazor();
           
            services.AddRequestLocalization<IOptions<BootstrapBlazorOptions>>((localizerOption, blazorOption) =>
            {
                var supportedCultures = blazorOption.Value.GetSupportedCultures();

                localizerOption.SupportedCultures = supportedCultures;
                localizerOption.SupportedUICultures = supportedCultures;
            });

            services.AddScoped(sp => new HttpClient());


            // 增加 Table Excel 导出服务
            services.AddBootstrapBlazorTableExcelExport();

            services.AddBlazoredLocalStorage();

            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAppHelper, AppHelper>();
            services.AddScoped(x => new ExamineService());
            services.AddScoped(x => new ImagesLargeViewService());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            //app.UseHttpsRedirection();
            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseStaticFiles();

            app.UseRouting();
            //客户端信息收集
            app.UseBootstrapBlazor();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
