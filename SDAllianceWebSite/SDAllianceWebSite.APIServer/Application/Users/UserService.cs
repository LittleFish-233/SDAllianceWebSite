using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.APIServer.Application.Users.Dtos;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.Shared.Application.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using System;
using System.Collections.Concurrent;
using SDAllianceWebSite.APIServer.Application.Helper;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.Shared.ExamineModel;
using System.IO;
using Newtonsoft.Json;

namespace SDAllianceWebSite.APIServer.Application.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<ThirdPartyLoginInfor, long> _thirdPartyLoginInforRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<ApplicationUser>, string, SortOrder, IEnumerable<ApplicationUser>>> SortLambdaCacheApplicationUser = new();
        public UserService(IRepository<ApplicationUser, string> userRepository, IConfiguration configuration, IHttpClientFactory clientFactory, IRepository<ThirdPartyLoginInfor, long> thirdPartyLoginInforRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _clientFactory = clientFactory;
            _thirdPartyLoginInforRepository = thirdPartyLoginInforRepository;
        }

        public async Task<PagedResultDto<ApplicationUser>> GetPaginatedResult(GetUserInput input)
        {
            var query = _userRepository.GetAll().AsNoTracking();

            //判断输入的查询名称是否为空
            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                query = query.Where(s => s.UserName.Contains(input.FilterText)
                  || s.MainPageContext.Contains(input.FilterText)
                  || s.PersonalSignature.Contains(input.FilterText));
            }
            //统计查询数据的总条数
            var count = query.Count();
            //根据需求进行排序，然后进行分页逻辑的计算
            query = query.OrderBy(input.Sorting).Skip((input.CurrentPage - 1) * input.MaxResultCount).Take(input.MaxResultCount);

            //将结果转换为List集合 加载到内存中
            List<ApplicationUser> models = null;
            if (count != 0)
            {
                models = await query.AsNoTracking().Include(s => s.Examines).ToListAsync();
            }
            else
            {
                models = new List<ApplicationUser>();
            }


            var dtos = new PagedResultDto<ApplicationUser>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting,
                ScreeningConditions = input.ScreeningConditions
            };

            return dtos;
        }

        public Task<QueryData<ListUserAloneModel>> GetPaginatedResult(QueryPageOptions options, ListUserAloneModel searchModel)
        {
            IEnumerable<ApplicationUser> items = _userRepository.GetAll().AsNoTracking();
            // 处理高级搜索
            if (!string.IsNullOrWhiteSpace(searchModel.UserName))
            {
                items = items.Where(item => item.UserName?.Contains(searchModel.UserName, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (!string.IsNullOrWhiteSpace(searchModel.PersonalSignature))
            {
                items = items.Where(item => item.PersonalSignature?.Contains(searchModel.PersonalSignature, StringComparison.OrdinalIgnoreCase) ?? false);
            }
            if (!string.IsNullOrWhiteSpace(searchModel.Email))
            {
                items = items.Where(item => item.Email?.Contains(searchModel.Email, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            // 处理 Searchable=true 列与 SeachText 模糊搜索
            if (options.Searchs.Any())
            {

                // items = items.Where(options.Searchs.GetFilterFunc<Entry>(FilterLogic.Or));
            }
            else
            {
                // 处理 SearchText 模糊搜索
                if (!string.IsNullOrWhiteSpace(options.SearchText))
                {
                    items = items.Where(item => (item.UserName?.Contains(options.SearchText) ?? false)
                                 || (item.PersonalSignature?.Contains(options.SearchText) ?? false)
                                  || (item.Email?.Contains(options.SearchText) ?? false));
                }
            }
            // 过滤
            /* var isFiltered = false;
             if (options.Filters.Any())
             {
                 items = items.Where(options.Filters.GetFilterFunc<Entry>());
                 isFiltered = true;
             }*/

            // 排序
            var isSorted = false;
            if (!string.IsNullOrWhiteSpace(options.SortName))
            {
                // 外部未进行排序，内部自动进行排序处理
                var invoker = SortLambdaCacheApplicationUser.GetOrAdd(typeof(ApplicationUser), key => LambdaExtensions.GetSortLambda<ApplicationUser>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            //复制数据
            List<ListUserAloneModel> resultItems = new List<ListUserAloneModel>();
            foreach (var item in items)
            {
                resultItems.Add(new ListUserAloneModel
                {
                    Id = item.Id,
                    UserName = item.UserName,
                    Email = item.Email,
                    PersonalSignature = item.PersonalSignature,
                    Birthday = item.Birthday,
                    RegistTime = item.RegistTime,
                    Integral = item.Integral,
                    CanComment = item.CanComment,
                    LearningValue = item.LearningValue,
                    OnlineTime = (double)item.OnlineTime / (60 * 60),
                    LastOnlineTime = item.LastOnlineTime,
                    UnsealTime = item.UnsealTime,
                    DisplayIntegral = item.DisplayIntegral,
                    DisplayLearningValue = item.DisplayLearningValue,
                    IsPassedVerification = item.IsPassedVerification,
                    IsShowFavotites = item.IsShowFavotites
                });
            }

            return Task.FromResult(new QueryData<ListUserAloneModel>()
            {
                Items = resultItems,
                TotalCount = total,
                IsSorted = isSorted,
                // IsFiltered = isFiltered
            });
        }

        #region 三方登入 计划重做

        public async Task<bool> AddUserMicrosoftThirdPartyLogin(string id_token, ApplicationUser user)
        {
           
            string result = ToolHelper.Base64DecodeString(id_token.Split('.')[1]);
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(result);
            string name = obj["name"].ToString();
            string email = obj["email"].ToString();
            string id = obj["oid"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.Microsoft);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.Microsoft;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        public async Task<ApplicationUser> GetMicrosoftThirdPartyLoginUser(string id_token)
        {

            string result = ToolHelper.Base64DecodeString(id_token.Split('.')[1]);
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(result);
            string id = obj["oid"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s=>s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId==id && s.Type == ThirdPartyLoginType.Microsoft);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        public async Task<string> GetMicrosoftThirdPartyLoginIdToken(string code,string returnUrl)
        {
            using var content = new MultipartFormDataContent();
            string client_id = _configuration["ThirdPartyLoginMicrosoft_client_id"];
            string client_secret = _configuration["ThirdPartyLoginMicrosoft_client_secret"];
            string resource = _configuration["ThirdPartyLoginMicrosoft_resource"];
            string tenant = _configuration["ThirdPartyLoginMicrosoft_tenant"];
            content.Add(
                content: new StringContent(client_id),
                name: "client_id");
            content.Add(
                content: new StringContent("authorization_code"),
                name: "grant_type");
            content.Add(
                content: new StringContent(returnUrl),
                name: "redirect_uri");
            content.Add(
                content: new StringContent(client_secret),
                name: "client_secret");
            content.Add(
                content: new StringContent(resource),
                name: "resource");
            content.Add(
                content: new StringContent(code),
                name: "code");

            HttpClient client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://login.microsoftonline.com/" + tenant + "/oauth2/token", content);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }
            JObject obj = JObject.Parse(newUploadResults);
            var id_token = obj["id_token"].ToString();
            if (string.IsNullOrWhiteSpace(id_token))
            {
                return null;
            }
            //解码返回的信息

            return id_token;
        }


        public async Task<bool> AddUserGithubThirdPartyLogin(string id_token, ApplicationUser user)
        {

            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            string name = obj["name"].ToString();
            //string email = obj["email"].ToString();
            string id = obj["id"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.GitHub);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            //item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.GitHub;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        public async Task<ApplicationUser> GetGithubThirdPartyLoginUser(string id_token)
        {

            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            string id = obj["id"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.GitHub);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        public async Task<string> GetGithubThirdPartyLoginIdToken(string code, string returnUrl,bool isSSR)
        {
            using var content = new MultipartFormDataContent();
            string client_id;
            string client_secret;
            if (isSSR)
            {
                client_id = _configuration["ThirdPartyLoginGithub_SSR_client_id"];

                client_secret = _configuration["ThirdPartyLoginGithub_SSR_client_secret"];
            }
            else
            {
                client_id = _configuration["ThirdPartyLoginGithub_WASM_client_id"];

                client_secret = _configuration["ThirdPartyLoginGithub_WASM_client_secret"];
            }

            content.Add(
                content: new StringContent(client_id),
                name: "client_id");
            content.Add(
                content: new StringContent(returnUrl),
                name: "redirect_uri");
            content.Add(
                content: new StringContent(client_secret),
                name: "client_secret");
            content.Add(
                content: new StringContent(code),
                name: "code");

            HttpClient client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }

            string access_token = ToolHelper.MidStrEx(newUploadResults, "access_token=", "&");

            if(string.IsNullOrWhiteSpace(access_token))
            {
                return null;
            }
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token",access_token);
            //client.DefaultRequestHeaders.Add("Authorization", "token "+access_token);
            var re = await client.GetAsync("https://api.github.com/user");
            var id_token = await re.Content.ReadAsStringAsync();

            return id_token;
        }

        public async Task<bool> AddUserGiteeThirdPartyLogin(string id_token, ApplicationUser user)
        {
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            string name = obj["name"].ToString();
            //string email = obj["email"].ToString();
            string id = obj["id"].ToString();
            //查找是否已经存在三方登入信息 存在则覆盖
            var item = await _thirdPartyLoginInforRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.Type == ThirdPartyLoginType.Gitee);
            if (item == null)
            {
                item = new ThirdPartyLoginInfor();
            }

            //item.Email = email;
            item.ApplicationUser = user;
            item.ApplicationUserId = user.Id;
            item.Name = name;
            item.Type = ThirdPartyLoginType.Gitee;
            item.UniqueId = id;

            if (item.Id == 0)
            {
                await _thirdPartyLoginInforRepository.InsertAsync(item);
            }
            else
            {
                await _thirdPartyLoginInforRepository.UpdateAsync(item);
            }

            return true;
        }

        public async Task<ApplicationUser> GetGiteeThirdPartyLoginUser(string id_token)
        {
            //获取名称 电子邮件 Id
            var obj = JObject.Parse(id_token);
            string id = obj["id"].ToString();

            var item = await _thirdPartyLoginInforRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).FirstOrDefaultAsync(s => s.UniqueId == id && s.Type == ThirdPartyLoginType.Gitee);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.ApplicationUser;
            }
        }

        public async Task<string> GetGiteeThirdPartyLoginIdToken(string code, string returnUrl)
        {
            using var content = new MultipartFormDataContent();
            string client_id;
            string client_secret;

            client_id = _configuration["ThirdPartyLoginGitee_client_id"];

            client_secret = _configuration["ThirdPartyLoginGitee_client_secret"];

            HttpClient client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://gitee.com/oauth/token?grant_type=authorization_code&code="+ code+"&client_id="+ client_id + "&redirect_uri="+ returnUrl+ "&client_secret=" + client_secret,content);
            if (response.IsSuccessStatusCode == false)
            {
                return null;
            }
            var newUploadResults = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(newUploadResults))
            {
                return null;
            }


            var obj = JObject.Parse(newUploadResults);
            string access_token = obj["access_token"].ToString();

            if (string.IsNullOrWhiteSpace(access_token))
            {
                return null;
            }
            var id_token = await client.GetStringAsync("https://gitee.com/api/v5/user?access_token="+access_token);

            return id_token;
        }
        #endregion

        public Task UpdateUserDataMain(ApplicationUser user, UserMain examine)
        {
            user.UserName = examine.UserName;
            user.PersonalSignature = examine.PersonalSignature;
            user.BackgroundImage = examine.BackgroundImage;
            user.PhotoPath = examine.PhotoPath;
            user.Institute = examine.Institute;
            user.StudentClass = examine.StudentClass;
            user.StudentId = examine.StudentId;
            user.StudentName = examine.StudentName;
            user.QQ = examine.QQ;
            user.WeChat = examine.WeChat;
            user.PublicEmail = examine.PublicEmail;
            return Task.CompletedTask;
        }

        public Task UpdateUserDataMainPage(ApplicationUser user, string examine)
        {
            user.MainPageContext = examine;
            return Task.CompletedTask;
        }

        public Task UpdateUserData(ApplicationUser user, Examine examine)
        {
            switch (examine.Operation)
            {
                case Operation.EditUserMain:
                    UserMain userMain = null;
                    using (TextReader str = new StringReader(examine.Context))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        userMain = (UserMain)serializer.Deserialize(str, typeof(UserMain));
                    }

                    UpdateUserDataMain(user, userMain);
                    break;
                case Operation.UserMainPage:
                    UpdateUserDataMainPage(user, examine.Context);
                    break;
            }
            return Task.CompletedTask;

        }

    }
}
