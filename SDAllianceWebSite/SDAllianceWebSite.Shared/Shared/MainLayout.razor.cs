using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using SDAllianceWebSite.Shared.Component.Others;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Pages.Home;
using SDAllianceWebSite.Shared.Service;
using SDAllianceWebSite.Shared.ViewModel.Space;
using SDAllianceWebSite.Shared.ViewModel.Theme;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Shared
{
    /// <summary>
    /// 
    /// </summary>
    public sealed partial class MainLayout
    {
        public bool UseTabSet { get; set; } = false;

        public string Theme { get; set; } = "color5";

        public bool IsOpen { get; set; }

        public bool IsFixedHeader { get; set; } = true;

        public bool IsFixedFooter { get; set; } = false;

        public bool IsFullSide { get; set; } = true;

        public bool ShowFooter { get; set; } = true;

        public bool IsDark { get; set; } = false;

        public bool IsDebug { get; set; } = false;

        public bool IsOnMouse { get; set; } = false;

        public bool IsOnBgImage { get; set; } = false;

        public UserUnReadedMessagesModel UnreadedMessages { get; set; } = new UserUnReadedMessagesModel();

        private List<MenuItem> Menus { get; set; }

        private Dictionary<string, string> TabItemTextDictionary { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }
      
        [NotNull]
        private StyleTip styleTip { get; set; }
        string image = null;
        string userId = null;
        string userName = null;

        #region 获取屏幕大小

        /// <summary>
        /// 获得/设置 IJSRuntime 实例
        /// </summary>
        [Inject]
        [System.Diagnostics.CodeAnalysis.NotNull]
        public IJSRuntime JSRuntime { get; set; }



        public bool IsSmallScreen { get; set; }
        public bool IsNormalScreen { get; set; }
        public bool IsLargeScreen { get; set; }


        private JSInterop<MainLayout> Interop { get; set; }
        /// <summary>
        /// OnAfterRenderAsync 方法
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                Interop = new JSInterop<MainLayout>(JSRuntime);
                await Interop.InvokeVoidAsync(this, null, "bb_layout", nameof(SetCollapsed));

                //读取本地主题配置
                await LoadTheme();
                //加载调试工具
                if (IsDebug)
                {
                    try
                    {
                        await JS.InvokeAsync<string>("initDebugTool");
                    }
                    catch
                    {


                    }
                }
                if (IsOnMouse)
                {
                    try
                    {
                        await JS.InvokeAsync<string>("InitMouse");
                    }
                    catch
                    {

                    }
                }



                //需要调用一次令牌刷新接口 确保登入没有过期
                var result = await _authService.Refresh();
                if (result != null && result.Code != LoginResultCode.OK)
                {
                    await ToastService.Error("尝试登入失败", result.ErrorDescribe);
                    StateHasChanged();
                }

                //var authState = await authenticationStateTask;
                //var user = authState.User;
                //获取用户信息
                //GetUserInfor(user);
                StateHasChanged();
                //读取用户未读信息
                await GetUserUnreadedMessages();

            }


            //删除多余的滚动条
            try
            {
                await JS.InvokeAsync<string>("deleteDiv", "slimScrollBar");
            }
            catch
            {

            }
        }



        /// <summary>
        /// 设置侧边栏收缩方法 客户端监控 window.onresize 事件回调此方法
        /// </summary>
        /// <returns></returns>
        [JSInvokable]
        public void SetCollapsed(int width)
        {
            if (IsSmallScreen != (width < 682))
            {
                IsSmallScreen = width < 682;
                StateHasChanged();
            }
            if (IsNormalScreen != (width >= 682 && width < 768))
            {
                IsNormalScreen = width >= 682 && width < 768;
                StateHasChanged();
            }
            if (IsLargeScreen != (width >= 768))
            {
                IsLargeScreen = width >= 768;
                StateHasChanged();
            }
        }
        #endregion

        /// <summary>
        /// 读取本地主题配置 并刷新
        /// </summary>
        /// <returns></returns>
        public async Task LoadTheme()
        {

            ThemeModel theme = await _localStorage.GetItemAsync<ThemeModel>("theme");
            if (theme == null)
            {
                return;
            }

            UseTabSet = theme.UseTabSet;
            Theme = theme.Theme;
            IsOpen = theme.IsOpen;
            IsFixedHeader = theme.IsFixedHeader;
            IsFixedFooter = theme.IsFixedFooter;
            IsFullSide = theme.IsFullSide;
            ShowFooter = theme.ShowFooter;
            IsDark = theme.IsDark;
            IsDebug = theme.IsDebug;
            IsOnMouse = theme.IsOnMouse;
            IsOnBgImage = theme.IsOnBgImage;

            StateHasChanged();
        }

        /// <summary>
        /// 保存本地主题配置
        /// </summary>
        /// <returns></returns>
        public async Task SaveTheme()
        {
            ThemeModel theme = new ThemeModel
            {
                UseTabSet = UseTabSet,
                Theme = Theme,
                IsOpen = IsOpen,
                IsFixedHeader = IsFixedHeader,
                IsFixedFooter = IsFixedFooter,
                IsFullSide = IsFullSide,
                ShowFooter = ShowFooter,
                IsDark = IsDark,
                IsDebug = IsDebug,
               IsOnMouse = IsOnMouse,
               IsOnBgImage= IsOnBgImage
            };
            await _localStorage.SetItemAsync("theme", theme);
        }
        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            //清楚登入令牌
            ToolHelper.LoginKey = "";
            ToolHelper.IsOnThirdPartyLogin = true;
            ToolHelper.ThirdPartyLoginTempModel = null;

          var authState = await authenticationStateTask;
            var user = authState.User;

            // 菜单获取可以通过数据库获取，此处为示例直接拼装的菜单集合
            TabItemTextDictionary = new()
            {
                [""] = "主页",
                ["/entries/index"] = "词条",
            };
            //获取菜单
            Menus = GetIconSideMenuItemsAsync(user);

            //启动定时器
            mytimer = new System.Threading.Timer(new System.Threading.TimerCallback(Send), null, 0, 1000 * 60 * 10);

         
        }
        private void GetUserInfor(ClaimsPrincipal user)
        {
            foreach (var item in user.Claims)
            {
                if (item.Type == "image")
                {
                    image = item.Value;
                }
                else if(item.Type=="userid")
                {
                    userId = item.Value;
                }
            }
        }

        private static List<MenuItem> GetIconSideMenuItemsAsync(ClaimsPrincipal user)
        {
            var menus = new List<MenuItem>
            {
                new MenuItem() { Text = "主页", Icon = "fa fa-fw fa-home", Url = "/" , Match = NavLinkMatch.All},               
                new MenuItem() { Text = "文章", Icon = "fa fa-fw fa-newspaper-o", Url = "/articles/home" },
              new MenuItem() { Text = "关于", Icon = "fa fa-fw fa-bolt", Url = "/home/about" },
              //new MenuItem() { Text = "测试", Icon = "fa fa-fw fa-bolt", Url = "/tests/index" },
            };

            if (user.IsInRole("Admin"))
            {
                menus.Insert(0,
                new MenuItem()
                {
                    IsCollapsed = true,
                    Text = "管理员后台",
                    Icon = "fa fa-fw fa-mortar-board",
                    Url="/null",
                    Items = new List<MenuItem>{
                              new MenuItem() { Text = "概览", Icon = "fa fa-fw fa-bar-chart-o", Url = "/admin/index" },
                              new MenuItem() { Text = "数据", Icon = "fa fa-fw fa-database", Url = "/admin/data" },
                              new MenuItem() { Text = "审核", Icon = "fa fa-fw fa-pencil", Url = "/admin/ListExamines" },
                              new MenuItem() { Text = "工具", Icon = "fa fa-fw fa-wrench", Url = "/admin/tools" },
                              new MenuItem() { Text = "备份", Icon = "fa fa-fw fa-cloud-upload", Url = "/admin/listbackuparchives" },
                              new MenuItem() { Text = "批量导入", Icon = "fa fa-fw fa-upload", Url = "/admin/importdata" },
                              new MenuItem() { Text = "定时任务", Icon = "fa fa-fw fa-tasks", Url = "/admin/listtimedtasks" },
                              new MenuItem() { Text = "管理评论", Icon = "fa fa-fw fa-comments-o", Url = "/admin/listcomments" },
                              new MenuItem() { Text = "管理消息", Icon = "fa fa-fw fa-envelope-o", Url = "/admin/listmessages" },
                              new MenuItem() { Text = "管理用户", Icon = "fa fa-fw  fa-user-circle", Url = "/admin/ListUsers" },
                              new MenuItem() { Text = "管理用户角色", Icon = "fa fa-fw fa-street-view", Url = "/admin/ListRoles" },
                              new MenuItem() { Text = "管理文章", Icon = "fa fa-fw fa-file-text-o", Url = "/admin/ListArticles" },
                              new MenuItem() { Text = "管理收藏", Icon = "fa fa-fw fa-folder-open", Url = "/admin/ListFavoriteFolders" },
                              new MenuItem() { Text = "管理图片", Icon = "fa fa-fw fa-image", Url = "/admin/ListImages" },
                              new MenuItem() { Text = "其他设置", Icon = "fa fa-fw fa-cog", Url = "/admin/ManageHome" }
                    }
                });
              
            }
            return menus;
        }

        System.Threading.Timer mytimer;

        public async void Send(object o)
        {
            try
            {
                var authState = await authenticationStateTask;
                var user = authState.User;
                if (user.Identity.IsAuthenticated)
                {
                    await InvokeAsync(() => Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/account/MakeUserOnline"));
                }
            }
            catch
            {

            }
        }

        public void OnSearch()
        {
            NavigationManager.NavigateTo("/home/search/全部", "搜索", "fa fa-search");
        }
        public void OnReturnHome()
        {
            NavigationManager.NavigateTo("/home/", "主页", "fa fa-home");
        }
        public Task Logout()
        {
            NavigationManager.NavigateTo("/account/logout/");
            return Task.CompletedTask;
        }

        public async Task NavigateToWASM()
        {
            await JS.InvokeAsync<string>("openNewPage", "https://app.ruanmeng.love/");
        }
        public async Task NavigateToSSR()
        {
            await JS.InvokeAsync<string>("openNewPage", "https://www.ruanmeng.love/");
        }

        public async Task GetUserUnreadedMessages()
        {
            if (string.IsNullOrWhiteSpace(userId) == false)
            {
                try
                {
                    UnreadedMessages = await Http.GetFromJsonAsync<UserUnReadedMessagesModel>(ToolHelper.WebApiPath + "api/space/GetUserUnReadedMessages/" + userId);
                    StateHasChanged();
                }
                catch
                {
                    UnreadedMessages = new UserUnReadedMessagesModel();
                }
            }

        }

        public async  Task OnClickMessage()
        {
            var authState = await authenticationStateTask;
            var user = authState.User;

            NavigationManager.NavigateTo("/space/index/" + userId, user.Identity.Name, "fa-star-o");
            //await OnReadedAllMessage();

            UnreadedMessages = new UserUnReadedMessagesModel();
        }
        public async Task OnReadedAllMessage()
        {
            try
            {
                var obj = await Http.GetFromJsonAsync<Result>(ToolHelper.WebApiPath + "api/space/ReadedAllMessages/");
                //判断结果
                if (obj.Successful == false)
                {
                    await ToastService.Error("使消息已读失败", obj.Error);
                }
            }
            catch
            {
                await ToastService.Error("使消息已读失败", "发生未知错误，请确保网络正常后联系开发人员");
            }
        }

    }
}
