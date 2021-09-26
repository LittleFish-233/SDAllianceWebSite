using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.Application.Roles.Dtos;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SDAllianceWebSite.Shared.Application.Roles
{
    public interface IRoleService
    {
        public PagedResultDto<IdentityRole> GetPaginatedResult(GetRoleInput input, List<IdentityRole> examines);

    }
}
