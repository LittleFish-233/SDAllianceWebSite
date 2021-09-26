using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Models;
using SDAllianceWebSite.APIServer.Application.Helper;
using SDAllianceWebSite.APIServer.DataReositories;
using SDAllianceWebSite.APIServer.ExamineX;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SDAllianceWebSite.Shared.Application.Dtos;
using SDAllianceWebSite.Shared.ViewModel.Search;
using SDAllianceWebSite.APIServer.Application.Files;
using SDAllianceWebSite.Shared.ViewModel.Files;
using SDAllianceWebSite.Shared.Helper;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using COSSTS;
using Microsoft.Extensions.Configuration;
using TencentCloud.Sts.V20180813.Models;
using Newtonsoft.Json.Linq;

namespace SDAllianceWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/files/[action]")]
    public class FilesAPIController : ControllerBase
    {
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IAppHelper _appHelper;

        public FilesAPIController(IRepository<UserFile, int> userFileRepository, IRepository<FileManager, int> fileManagerRepository, IAppHelper appHelper, IConfiguration configuration,
        IFileService fileService)
        {
            _appHelper = appHelper;
            _fileManagerRepository = fileManagerRepository;
            _userFileRepository = userFileRepository;
            _fileService = fileService;
            _configuration = configuration;
        }

        private async Task<ActionResult<List<UploadResult>>> PostFileMain(IEnumerable<IFormFile> files, double x = 0, double y = 0, int server = 0)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;
            foreach (var item in files)
            {
                total += item.Length;
            }
            if (total > 500 * 1024 * 1024)
            {
                return new List<UploadResult> { new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" } };
            }
            //加载文件管理
            FileManager fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize)
                {
                    return new List<UploadResult> { new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" } };
                }
            }

            var maxAllowedFiles = 10;
            long maxFileSize = 1024 * 1024 * 5;
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");

            List<UploadResult> results = new List<UploadResult>();
            foreach (var item in files)
            {
                var uploadResult = new UploadResult();
                if (filesProcessed < maxAllowedFiles)
                {
                    if (item.Length == 0)
                    {
                        //文件大小为0
                        uploadResult.Uploaded = false;
                        uploadResult.Error = "文件大小为0";
                    }
                    else if (item.Length > maxFileSize && server != 1)
                    {
                        //文件大小超过上限
                        uploadResult.Uploaded = false;
                        uploadResult.Error = "文件大小超过上限";
                    }
                    else
                    {
                        try
                        {
                            uploadResult.FileName = await _appHelper.UploadImageNewAsync(fileManager, item, x, y, server);
                            uploadResult.FileURL = _appHelper.GetImagePath(uploadResult.FileName, "");
                            uploadResult.Uploaded = true;
                        }
                        catch (IOException ex)
                        {
                            //文件没有被上传
                            uploadResult.Error = "文件没有被上传，异常详细信息：" + ex.Message;
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    uploadResult.Uploaded = false;
                    uploadResult.Error = "超过单次文件上传上限";
                }
                results.Add(uploadResult);
            }


            return results;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFileB([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 0, 0, 1);
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile16_9([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 460, 215);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile9_16([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 9, 16);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile1_1([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 1, 1);
        }
        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFile4_1A2([FromForm] IEnumerable<IFormFile> files)
        {
            return await PostFileMain(files, 4, 1.2);
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddUserLoadedFileInfor(ImageInforTipViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                return new Result { Successful = false, Error = "该用户不存在" };
            }
            //加载文件管理
            FileManager fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = 0,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (long)model.FileSize;
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);

            return new Result { Successful = true };

        }


        /// <summary>
        /// 删除文件 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpPost]
        public Task<ActionResult<bool>> DeleteFile([FromBody] string fileName)
        {
            //获取当前用户ID
            // ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            // return await _appHelper.DeleteFieAsync(user, fileName);
            return Task.FromResult<ActionResult<bool>>(true);
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result>> GetRandomImageAsync()
        {
            Random random = new Random();
            int length = await _userFileRepository.CountAsync();
            if (length > 0)
            {
                int p = random.Next(1, length - 1);
                var temp = await _userFileRepository.FirstOrDefaultAsync(s => s.Id == p);
                if (temp != null)
                {
                    return new Result { Successful = true, Error = _appHelper.GetImagePath(temp.FileName, "") };
                }
                else
                {
                    return new Result { Successful = false };
                }
            }
            else
            {
                return new Result { Successful = false };
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<PagedResultDto<ImageInforTipViewModel>> GetFileListAsync(PagedSortedAndFilterInput input)
        {
            return await _fileService.GetPaginatedResult(input);
        }


        [HttpPost]
        public async Task<ActionResult<UploadResult>> PostFileUrlBefore(ImageInforTipViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;

            if (model.FileSize > 50 * 1024 * 1024)
            {
                return new UploadResult { Uploaded = false, Error = "文件上限为 50 MB" };
            }

            //加载文件管理
            FileManager fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize)
                {
                    return new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" };
                }
            }
            //检查是否已经上传过该文件
            if (string.IsNullOrWhiteSpace(model.Sha1) == false)
            {
                //检查数据库中是否有相同的文件
                UserFile userFile = await _userFileRepository.FirstOrDefaultAsync(s => s.Sha1 == model.Sha1);
                if (userFile != null)
                {
                    return new UploadResult { Uploaded = true, FileName = userFile.FileName, FileURL = userFile.FileName };
                }
            }
            return new UploadResult { Uploaded = true };
        }

        [HttpPost]
        public async Task<ActionResult<UploadResult>> PostFileUrlAfter(ImageInforTipViewModel model)
        {
            //获取当前用户ID
            ApplicationUser user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断剩余空间
            long total = 0;

            if (model.FileSize > 50 * 1024 * 1024)
            {
                return new UploadResult { Uploaded = false, Error = "文件上限为 50 MB" };
            }

            //加载文件管理
            FileManager fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                fileManager = new FileManager
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    TotalSize = 500 * 1024 * 1024,
                    UsedSize = total,
                    UserFiles = new List<UserFile>()
                };
                fileManager = await _fileManagerRepository.InsertAsync(fileManager);
            }
            else
            {
                if (fileManager.UsedSize + total > fileManager.TotalSize)
                {
                    return new UploadResult { Uploaded = false, Error = "该用户总文件空间不足" };
                }
            }

            //检查通过后添加文件
            fileManager.UserFiles.Add(new UserFile
            {
                FileName = model.FileName,
                UploadTime = DateTime.Now.ToCstTime(),
                FileSize = model.FileSize,
                Sha1 = model.Sha1,
                UserId = fileManager.ApplicationUserId
            });
            fileManager.UsedSize += (long)model.FileSize;
            //更新用户文件列表
            await _fileManagerRepository.UpdateAsync(fileManager);

            return new UploadResult { Uploaded = true };

        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> PostFileBase64(ImageBase64UploadModel model)
        {
            //解码
            try
            {
                var temp = Convert.FromBase64String(model.Base64Str);
                using Stream stream = new MemoryStream(temp);
                IFormFile fromFile = new FormFile(stream, 0, stream.Length, "测试", "测试.png");
                //上传

                return await PostFileMain(new List<IFormFile> { fromFile }, model.Width, model.Height);
            }
            catch (Exception exc)
            {
                return new List<UploadResult> { new UploadResult { Error = "发生未知异常", Uploaded = false } };
            }

        }

        public async Task<ActionResult<FileTempCredentialModel>> GetTempCredential()
        {
            string bucket = _configuration["bucket"];  // 您的 bucket
            string region = _configuration["region"];  // bucket 所在区域
            string allowPrefix = "*"; // 这里改成允许的路径前缀，可以根据自己网站的用户登录态判断允许上传的具体路径，例子： a.jpg 或者 a/* 或者 * (使用通配符*存在重大安全风险, 请谨慎评估使用)
            string[] allowActions = new string[] {  // 允许的操作范围，这里以上传操作为例
                "name/cos:PutObject",
                "name/cos:PostObject",
                "name/cos:InitiateMultipartUpload",
                "name/cos:ListMultipartUploads",
                "name/cos:ListParts",
                "name/cos:UploadPart",
                "name/cos:CompleteMultipartUpload"
            };
            // Demo 这里是从环境变量读取，如果是直接硬编码在代码中，请参考：
            // string secretId = "AKIDXXXXXXXXX";
            string secretId = _configuration["secretId"];// 云 API 密钥 Id
            string secretKey = _configuration["secretKey"]; // 云 API 密钥 Key

            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("bucket", bucket);
            values.Add("region", region);
            values.Add("allowPrefix", allowPrefix);
            // 也可以通过 allowPrefixes 指定路径前缀的集合
            // values.Add("allowPrefixes", new string[] {
            //     "path/to/dir1/*",
            //     "path/to/dir2/*",
            // });
            values.Add("allowActions", allowActions);
            values.Add("durationSeconds", 1800);

            values.Add("secretId", secretId);
            values.Add("secretKey", secretKey);

            // 设置域名
            // values.Add("Domain", "sts.tencentcloudapi.com");

            // Credentials = {
            //   "Token": "4oztDXOAAI3c6qUE5TkNuVzSP1tUQz15f3f946eb08f9411d3d61505cc4bc74cczCZLchkvRmmrqzE09ELVw35gzYlBXsQp03PBpL79ubLvoAMWbBgSMmI6eApmhqv7NFeDdKJlikVe0fNCU2NNUe7cHrgttfTIK87ZnC86kww-HysFgIGeBNWpwo4ih0lV0z9a2WiTIjPoeDBwPU4YeeAVQAGPnRgHALoL2FtxNsutFzDjuryRZDK7Am4Cs9YxpZHhG7_F_II6363liKNsHTk8ONRZrNxKiOqvFvyhsJ-oTTUg0I0FT4_xo0lq5zR9yyySXHbE7z-2im4rgnK3sBagN47zkgltJyefJmaPUdDgGmvaQBO6TqxiiszOsayS7CxCZK1yi90H2KS3xRUYTLf94aVaZlufrIwntXIXZaHOKHmwuZuXl7HnHoXbfg_YENoLP6JAkDCw0GOFEGNOrkCuxRtcdJ08hysrwBw1hmYawDHkbyxYkirY-Djg7PswiC4_juBvG0iwjzVwE0W_rhxIa7YtamLnZJxQk9dyzbbl0F4DTYwS101Hq9wC7jtifkXFjBFTGRnfPe85K-hEnJLaEy7eYfulIPI9QiIUxi4BLPbzjD9j3qJ4Wdt5oqk9XcF9y5Ii2uQx1eymNl7qCA",
            //   "TmpSecretId": "xxxxxxxxxxxx",
            //   "TmpSecretKey": "PZ/WWfPZFYqahPSs8URUVMc8IyJH+T24zdn8V1cZaMs="
            // }
            // ExpiredTime = 1597916602
            // Expiration = 2020/8/20 上午9:43:22
            // RequestId = 2b731be1-ebe8-4638-8a72-906bc564a55a
            // StartTime = 1597914802

            FileTempCredentialModel model = new FileTempCredentialModel();

            try
            {
                Dictionary<string, object> credential = STSClient.genCredential(values);
                model.sessionToken = ((JObject)credential["Credentials"])["Token"].ToString();
                model.tmpSecretId = ((JObject)credential["Credentials"])["TmpSecretId"].ToString();
                model.tmpSecretKey = ((JObject)credential["Credentials"])["TmpSecretKey"].ToString();
                model.expiredTime = credential["ExpiredTime"].ToString();
                model.startTime = credential["StartTime"].ToString();
            }
            catch (Exception exc)
            {

            }


            return model;
        }


    }
}
