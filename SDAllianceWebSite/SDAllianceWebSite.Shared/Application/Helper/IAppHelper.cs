using SDAllianceWebSite.Shared.Model;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using SDAllianceWebSite.Shared.Component.Others.Images;

namespace SDAllianceWebSite.Shared.Application.Helper
{
    public interface IAppHelper
    {
        /// <summary>
        /// 批量上传文件 不附加参数
        /// </summary>
        /// <param name="files">文件列表</param>
        /// <param name="maxFileSize">文件最大大小</param>
        /// <param name="maxAllowedFiles">最大文件数</param>
        /// <param name="url">上传到的URL</param>
        /// <returns></returns>
        Task<List<UploadResult>> UploadFilesAsync(List<IBrowserFile> files, long maxFileSize, int maxAllowedFiles, string url);

        Task<List<UploadResult>> UploadFilesAsync(string base64, ImageAspectType type);

        Task<string> GetIPCountiy(string strIP);
    }
}
