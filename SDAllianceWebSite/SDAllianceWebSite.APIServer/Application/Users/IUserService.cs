
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.APIServer.Application.Users.Dtos;
using SDAllianceWebSite.Shared.Application.Dtos;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using BootstrapBlazor.Components;
using SDAllianceWebSite.Shared.ExamineModel;

namespace SDAllianceWebSite.APIServer.Application.Users
{
    public interface IUserService
    {
        Task<PagedResultDto<ApplicationUser>> GetPaginatedResult(GetUserInput input);

        Task<QueryData<ListUserAloneModel>> GetPaginatedResult(QueryPageOptions options, ListUserAloneModel searchModel);

        Task<bool> AddUserMicrosoftThirdPartyLogin(string id_token, ApplicationUser user);

        Task<ApplicationUser> GetMicrosoftThirdPartyLoginUser(string id_token);

        Task<string> GetMicrosoftThirdPartyLoginIdToken(string code, string returnUrl);

        Task<bool> AddUserGithubThirdPartyLogin(string id_token, ApplicationUser user);

        Task<ApplicationUser> GetGithubThirdPartyLoginUser(string id_token);

        Task<string> GetGithubThirdPartyLoginIdToken(string code, string returnUrl,bool isSSR);

        Task<bool> AddUserGiteeThirdPartyLogin(string id_token, ApplicationUser user);

        Task<ApplicationUser> GetGiteeThirdPartyLoginUser(string id_token);

        Task<string> GetGiteeThirdPartyLoginIdToken(string code, string returnUrl);

        Task UpdateUserDataMain(ApplicationUser user, UserMain examine);

        Task UpdateUserDataMainPage(ApplicationUser user, string examine);

        Task UpdateUserData(ApplicationUser user, Examine examine);

    }
}
