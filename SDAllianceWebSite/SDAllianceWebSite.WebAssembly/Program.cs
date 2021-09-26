using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SDAllianceWebSite.Shared;
using SDAllianceWebSite.Shared.Application.Examines;
using SDAllianceWebSite.Shared.Application.Helper;
using SDAllianceWebSite.Shared.Application.Roles;
using SDAllianceWebSite.Shared.Component.Others.Images;
using SDAllianceWebSite.Shared.Provider;
using SDAllianceWebSite.Shared.Service;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.WebAssembly
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            // 增加 BootstrapBlazor 组件
            builder.Services.AddBootstrapBlazor();

            // 增加 Table Excel 导出服务
            builder.Services.AddBootstrapBlazorTableExcelExport();

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IAppHelper, AppHelper>();
            builder.Services.AddScoped(x => new ExamineService());
            builder.Services.AddScoped(x => new ImagesLargeViewService());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
