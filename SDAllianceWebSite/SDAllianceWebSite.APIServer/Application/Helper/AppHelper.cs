using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.Model.ExamineModel;
using SDAllianceWebSite.Shared.Models;
using SDAllianceWebSite.APIServer.DataReositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NETCore.MailKit.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using SDAllianceWebSite.Shared.Helper;
using SDAllianceWebSite.Shared.ExamineModel;
using System.Security.Policy;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using SDAllianceWebSite.Shared.ViewModel.Accounts;
using SDAllianceWebSite.Shared.ViewModel.Admin;
using SDAllianceWebSite.Shared.ViewModel.Search;
using System.Net.Http.Json;
using System.Security.Cryptography;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using TencentCloud.Captcha.V20190722.Models;
using TencentCloud.Captcha.V20190722;
using TencentCloud.Common.Profile;
using TencentCloud.Common;
using Markdig;
using System.Linq.Dynamic.Core;
using System.Collections;
using TencentCloud.Ame.V20190916.Models;
using System.Data;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;

namespace SDAllianceWebSite.APIServer.Application.Helper
{
    public class AppHelper : IAppHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<TokenCustom, int> _tokenCustomRepository;
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<ErrorCount, long> _errorCountRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<TimedTask, int> _timedTaskRepository;
        private readonly IRepository<SendCount, long> _sendCountRepository;
        private readonly IRepository<Loginkey, long> _loginkeyRepository;
        private readonly IEmailService _EmailService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public AppHelper(IRepository<TimedTask, int> timedTaskRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRepository<SignInDay, long> signInDayRepository,
        IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteFolder, long> favoriteFolderRepository,
        IRepository<ErrorCount, long> errorCountRepository, IRepository<HistoryUser, int> historyUserRepository, IRepository<Message, long> messageRepository, IRepository<ApplicationUser, string> userRepository,
            IRepository<Comment, long> commentRepository, IConfiguration configuration, UserManager<ApplicationUser> userManager, IRepository<Loginkey, long> loginkeyRepository,
        IHttpClientFactory clientFactory, IRepository<FileManager, int> fileManagerRepository, IEmailService EmailService, IRepository<TokenCustom, int> tokenCustomRepository,
            IRepository<Article, long> aricleRepository, SignInManager<ApplicationUser> signInManager, IRepository<SendCount, long> sendCountRepository,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository )
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            _examineRepository = examineRepository;
            _articleRepository = aricleRepository;
            _EmailService = EmailService;
            this._webHostEnvironment = webHostEnvironment;
            _fileManagerRepository = fileManagerRepository;
            this._tokenCustomRepository = tokenCustomRepository;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
            _examineRepository = examineRepository;
            _messageRepository = messageRepository;
            _historyUserRepository = historyUserRepository;
            _errorCountRepository = errorCountRepository;
            _timedTaskRepository = timedTaskRepository;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _userFileRepository = userFileRepository;
            _signInDayRepository = signInDayRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _sendCountRepository = sendCountRepository;
            _loginkeyRepository = loginkeyRepository;
        }

        public string GetImagePath(string image, string defaultStr)
        {

            if (string.IsNullOrWhiteSpace(image) == true)
            {
                if (string.IsNullOrWhiteSpace(defaultStr) == true)
                {
                    return "";
                }
                else
                {
                    image = "default/" + defaultStr;
                    return "//app.ruanmeng.love/_content/SDAllianceWebSite.Shared/images/" + image;
                }

            }
            else
            {
                //判断是否为绝对路径
                if (image.Contains("http://") || image.Contains("https://") || image.Contains("//"))
                {
                    return image;
                }
                else
                {
                    //http://localhost:51313/
                    //121.43.54.210
                    return "//app.ruanmeng.love/_content/SDAllianceWebSite.Shared/images/" + image;
                }
            }
        }

        public string GetStringAbbreviation(string str, int length)
        {
            if (str == null)
            {
                return "";
            }
            if (str.Length < length * 2.5)
            {
                return str;
            }
            else
            {
                return string.Concat(str.AsSpan(0, (int)(length * 2.5)), "...");
            }
        }

        public async Task AddUserContributionValueAsync(string userId, long otherId, Operation operation)
        {
            //更新用户积分
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await UpdateUserIntegral(user);
            }
        }

        public async Task<bool> IsArticleLockedAsync(long articleId, string userId, Operation operation)
        {
            if (await _examineRepository.FirstOrDefaultAsync(s => s.ArticleId == articleId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> CanUserEditArticleAsync(ApplicationUser user, long articleId)
        {
            //读取当前文章的审核信息 获取最后编辑记录和作者
            //判断当前用户是否有权限编辑
            if (await _userManager.IsInRoleAsync(user, "Admin") == true)
            {
                return true;
            }
            var sum = await _examineRepository.GetAll().CountAsync(s => s.ArticleId == articleId && s.IsPassed != false && s.ApplicationUserId == user.Id);
            if (sum > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<int> GetShortTokenAsync(string longToken, string userName)
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();

            if (longToken == null)
            {
                //按用户名查找验证码
                TokenCustom tokenCustom = await _tokenCustomRepository.GetAll().OrderBy("Time desc").FirstOrDefaultAsync(s => s.UserName == userName && s.Time != null && ((DateTime)s.Time).AddHours(1) > tempDateTimeNow);
                if (tokenCustom != null)
                {
                    return tokenCustom.Num.Value;
                }
                else
                {
                    return 0;
                }
            }

            int result = 0;
            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                result = random.Next(100000, 999999);
                TokenCustom tokenCustom = await _tokenCustomRepository.FirstOrDefaultAsync(s => s.Num != null && s.Num == result);
                if (tokenCustom == null)
                {
                    await _tokenCustomRepository.InsertAsync(new TokenCustom
                    {
                        Num = result,
                        Token = longToken,
                        Time = tempDateTimeNow,
                        UserName = userName
                    });
                    break;
                }
                else
                {
                    if (tokenCustom.Time != null && ((DateTime)tokenCustom.Time).AddHours(1) < tempDateTimeNow)
                    {
                        tokenCustom.Token = longToken;
                        tokenCustom.Num = result;
                        tokenCustom.Time = tempDateTimeNow;
                        tokenCustom.UserName = userName;
                        await _tokenCustomRepository.UpdateAsync(tokenCustom);
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<string> GetLongTokenAsync(string shortToken)
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            TokenCustom token = await _tokenCustomRepository.FirstOrDefaultAsync(s => (s.Num != null && s.Num.ToString() == shortToken) && (s.Time != null && ((DateTime)s.Time).AddHours(1) > tempDateTimeNow));
            if (token != null)
            {
                string temp = token.Token;
                // await _tokenCustomRepository.DeleteAsync(token);
                return temp;
            }
            return null;
        }

        public async Task<string> SendVerificationEmailAsync(string longToken, string email, string userName)
        {
            //检查是否超过上限
            if (await IsExceedMaxSendCount(email, 10, 10))
            {
                return "验证码超过发送次数上限";
            }
            try
            {
                int num = await GetShortTokenAsync(longToken, userName);
                if (num == 0)
                {
                    return null;
                }
                _EmailService.Send(email, "验证你的软件开发联盟账号", "<div	id='contentDiv'	onmouseover='getTop().stopPropagation(event);'	onclick='getTop().preSwapLink(event, 'spam', 'ZC2901-OBne295lonzXhDxipYEzca5');'	style='		position: relative; font-size: 14px; height: auto; padding: 15px 15px 10px 15px; z-index: 1; zoom: 1; line-height: 1.7;	'	class='body'>	<div id='qm_con_body'>		<div			id='mailContentContainer'			class='qmbox qm_con_body_content qqmail_webmail_only'		>			<style>				.qmbox .btn:before,				.qmbox .msg-heart:after,				.qmbox .msg-star:after,				.qmbox .msg-plus:after,				.qmbox .msg-file:before {					speak: none;					font-size: 25px;					font-style: normal;					font-variant: normal;					text-transform: none;					line-height: 1;					position: relative;					-webkit-font-smoothing: antialiased;				}				.qmbox .btn {					text-decoration: none;					border: 0;					font-family: inherit;					font-size: inherit;					color: inherit;					background: 0;					cursor: pointer;					padding: 15px 15px;					display: inline-block;					text-transform: uppercase;					letter-spacing: 1px;					font-weight: 700;					outline: 0;					position: relative;					-webkit-transition: all 0.3s;					-moz-transition: all 0.3s;					transition: all 0.3s;					background: #393;					color: #fff;				}				.qmbox .btn:after {					background: #393;					content: '';					position: absolute;					z-index: -1;					-webkit-transition: all 0.3s;					-moz-transition: all 0.3s;					transition: all 0.3s;				}			</style>			<div				style='					background-position: bottom; background-color: #ececec;					padding: 35px;				'			>				<table					cellpadding='0'					align='center'					style='						width: 600px;						margin: 0px auto;						text-align: left;						position: relative;						border-top-left-radius: 5px;						border-top-right-radius: 5px;						border-bottom-right-radius: 5px;						border-bottom-left-radius: 5px;						font-size: 14px;						font-family: 微软雅黑, 黑体;						line-height: 1.5;						box-shadow: rgb(153, 153, 153) 0px 0px 5px;						border-collapse: collapse;						background-position: initial initial;						background-repeat: initial initial;						background: #fff;					'				>					<tbody>						<tr>							<th								valign='middle'								style='									height: 25px;									line-height: 25px;									padding: 15px 35px;									border-bottom-width: 1px;									border-bottom-style: solid;									border-bottom-color: #17a2b8;									background-color: #17a2b8;									border-top-left-radius: 5px;									border-top-right-radius: 5px;									border-bottom-right-radius: 0px;									border-bottom-left-radius: 0px;									text-align: center;font-size: x-large;color: white;								'							>软件开发联盟</th>						</tr>						<tr>							<td>								<div									style='										padding: 25px 35px 40px;										background-color: #fff;										max-width: 550px;									'								>									<h2 style='margin: 5px 0px'>										<font color='#333333' style='line-height: 20px'>											<font style='line-height: 22px' size='4'>												亲爱的 " + userName + "</font											>										</font>									</h2>									<p>										非常感谢您的访问，欢迎来到 软件开发联盟，在完成此部分前，我们需要先验证一下您的邮箱哦。<br />							<br />	<h1>验证码：" + num + "</h1><br />			<br />								最后，感谢您的访问，祝您使用愉快！									</p>									<p align='right'>软件开发联盟 管理团队</p>									<div style='width: 550px; margin: 0 auto'>										<div											style='												padding: 10px 10px 0;												border-top: 1px solid #ccc;												color: #747474;												margin-bottom: 20px;												line-height: 1.3em;												font-size: 12px;											'										>											<p>												此为系统邮件，请勿回复<br />												请保管好您的邮箱，避免账号被他人盗用											</p>											<p>												©ruanmeng.love<br />												<a													href='https://www.ruanmeng.love/'													rel='noopener'													target='_blank'													>https://www.ruanmeng.love/</a												>											</p>										</div>									</div>								</div>							</td>						</tr>					</tbody>				</table>			</div>			<style type='text/css'>				.qmbox style,				.qmbox script,				.qmbox head,				.qmbox link,				.qmbox meta {					display: none !important;				}			</style>		</div>	</div>	<!-- --><style>		#mailContentContainer .txt {			height: auto;		}	</style></div>", true);
                //增加发送次数
                await AddSendCount(email, SendType.Email);
                return null;
            }
            catch (Exception exc)
            {
                return exc.Message;
            }
        }


        public async Task<string> SendVerificationSMSAsync(string longToken, string phone, string userName, SMSType type)
        {
            //检查是否超过上限
            if (await IsExceedMaxSendCount(phone, 10, 10))
            {
                return "验证码超过发送次数上限";
            }
            try
            {
                int num = await GetShortTokenAsync(longToken, userName);
                if (num == 0)
                {
                    return null;
                }
                AlibabaCloud.SDK.Dysmsapi20170525.Client client = CreateClient(_configuration["AccessKeyId"], _configuration["AccessKeySecret"]);
                AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
                {
                    PhoneNumbers = phone,
                    SignName = "cngal",
                    TemplateCode = GetSMSCodeFromSMSType(type),
                    TemplateParam = "{ 'code':'" + num.ToString() + "'}",
                };

                // 复制代码运行请自行打印 API 的返回值
                var result = client.SendSms(sendSmsRequest);
                if (result.Body.Code == "OK")
                {
                    //增加发送次数
                    await AddSendCount(phone, SendType.Phone);
                    return null;
                }
                else
                {
                    return result.Body.Message;
                }

            }
            catch (Exception exc)
            {
                return exc.Message;
            }
        }

        public static string GetSMSCodeFromSMSType(SMSType type)
        {
            return type switch
            {
                SMSType.Register => "SMS_223580709",
                SMSType.ForgetPassword => "SMS_223585679",
                SMSType.ChangeMobilePhoneNumber => "SMS_223585685",
                _ => null,
            };
        }

        /// <summary>
        /// 初始化账号Client
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <returns></returns>
        public static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret)
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 您的AccessKey ID
                AccessKeyId = accessKeyId,
                // 您的AccessKey Secret
                AccessKeySecret = accessKeySecret,
            };
            // 访问的域名
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
        }

        public async Task<ApplicationUser> GetAPICurrentUserAsync(HttpContext context)
        {
            var test = context.Request.Path;
            string bearer = context.Request.Headers["Authorization"].FirstOrDefault();
            ApplicationUser result = null;
            if (string.IsNullOrWhiteSpace(bearer) || !bearer.Contains("Bearer"))
            {
                try
                {
                    string name = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;
                    //获取当前用户ID
                    result = await _userManager.FindByNameAsync(name);
                }
                catch
                {

                }
                return null;
            }
            else
            {
                string[] jwt = bearer.Split(' ');
                var tokenObj = new JwtSecurityToken(jwt[1]);

                var claimsIdentity = new ClaimsIdentity(tokenObj.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                context.User = claimsPrincipal;

                string name = context.User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Name).Value;

                //获取当前用户ID
                result = await _userManager.FindByNameAsync(name);
            }

            //判断是否被封禁
            if (result != null)
            {
                if (result.UnsealTime == null)
                {
                    return result;
                }
                else
                {
                    if (result.UnsealTime < DateTime.Now.ToCstTime())
                    {
                        result.UnsealTime = null;
                        await _userManager.UpdateAsync(result);
                        return result;
                    }
                }
            }

            return null;

        }

        public async Task<string> UploadImageNewAsync(FileManager fileManager, IFormFile image, double x = 0, double y = 0, int server = 0, string path = "", string name = "")
        {

            //获取保存路径
            string uploadsFolder;
            string uniqueFileName;
            string filePath;
            string tempFilePath;
            if (path == "")
            {
                uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "images");
                string[] temp = image.FileName.Split('.');
                uniqueFileName = Guid.NewGuid().ToString() + "." + temp[^1];// + "_" + image.FileName;

            }
            else
            {
                uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, path);
                uniqueFileName = name;
            }
            filePath = Path.Combine(uploadsFolder, uniqueFileName);
            tempFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "temp", "imageprogress", uniqueFileName);
            //获取图片大小
            Bitmap source = null;
            try
            {
                source = new Bitmap(image.OpenReadStream());

            }
            catch
            {
                throw new Exception("请告知管理员：未安装libgdiplus");
            }
            int width = source.Width;
            int height = source.Height;

            //裁剪图片
            if (x != 0 && y != 0)
            {
                int linshi_x, linshi_y;

                linshi_x = source.Width;
                linshi_y = (int)((double)linshi_x / x * y);
                if (source.Height < linshi_y)
                {
                    linshi_y = source.Height;
                    linshi_x = (int)((double)linshi_y / y * x);
                }

                RectangleF rectangleF = new RectangleF(0, 0, linshi_x, linshi_y);
                Bitmap newBitmap = source.Clone(rectangleF, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                //保存裁剪的图片
                newBitmap.Save(tempFilePath);

                newBitmap.Dispose();
                width = linshi_x;
                height = linshi_y;
            }
            else
            {
                source.Save(tempFilePath);
            }

            source.Dispose();

            //再压缩图片
            if ((new FileInfo(tempFilePath)).Length > 500 * 1024 && server != 1)
            {
                GetPicThumbnail(File.OpenRead(tempFilePath), filePath, height, width, 50);
            }
            else
            {
                File.Copy(tempFilePath, filePath);
            }
            // 删除临时文件
            // File.Delete(tempFilePath);

            //检查是否已经上传过该文件
            string sha1 = ToolHelper.ComputeFileSHA1(filePath);
            if (string.IsNullOrWhiteSpace(sha1) == false)
            {
                //检查数据库中是否有相同的文件
                UserFile userFile = await _userFileRepository.FirstOrDefaultAsync(s => s.Sha1 == sha1);
                if (userFile != null)
                {
                    return userFile.FileName;
                }
            }

            //上传文件到远程服务器
            string UploadResults = null;
            if (server == 0)
            {
                string[] temp = image.FileName.Split('.');

                UploadResults = await UploadFileToBserver(filePath,temp[0],temp[1]);

            }
            else if (server == 1)
            {
                string[] temp = image.FileName.Split('.');

                UploadResults = await UploadFileToBserver(filePath, temp[0], temp[1]);
            }

            if (string.IsNullOrWhiteSpace(UploadResults) == false)
            {
                uniqueFileName = UploadResults;
                // 再填充用户文件管理
                FileInfo fileInfo = new FileInfo(filePath);
                long length = fileInfo.Length;

                fileManager.UserFiles.Add(new UserFile
                {
                    FileName = uniqueFileName,
                    UploadTime = DateTime.Now.ToCstTime(),
                    FileSize = length,
                    Sha1 = sha1,
                    UserId = fileManager.ApplicationUserId
                });
                fileManager.UsedSize += length;
                //更新用户文件列表
                await _fileManagerRepository.UpdateAsync(fileManager);

                return uniqueFileName;
            }
            else
            {
                throw new Exception("上传文件到远程服务器失败，请尽快联系管理员");
            }
        }

        private async Task<string> UploadFileToAserver(string filePath)
        {
            using var content = new MultipartFormDataContent();

            content.Add(
                content: new StringContent(GetFileBase64(filePath)),
                name: "content");
            content.Add(
                 content: new StringContent(_configuration["CnGalImageAPIToken"]),
                 name: "api_token");

            HttpClient client = _clientFactory.CreateClient();

            var response = await client.PostAsync("https://www.ruanmeng.love/api/upload/image", content);

            var newUploadResults = await response.Content.ReadFromJsonAsync<UploadImageResult>();
            if (newUploadResults != null && newUploadResults.code == 1 && string.IsNullOrWhiteSpace(newUploadResults.url) == false)
            {
                return newUploadResults.url;
            }
            else
            {
                return null;
            }
        }

        private async Task<string> UploadFileToBserver(string filePath,string fileName,string fileAddName)
        {
            //初始化 CosXmlConfig 
            string appid = _configuration["appid"];//设置腾讯云账户的账户标识 APPID
            string region = _configuration["region"]; //设置一个默认的存储桶地域
            CosXmlConfig config = new CosXmlConfig.Builder()
              .IsHttps(true)  //设置默认 HTTPS 请求
              .SetRegion(region)  //设置一个默认的存储桶地域
              .SetDebugLog(true)  //显示日志
              .Build();  //创建 CosXmlConfig 对象

            string secretId = _configuration["secretId"]; //"云 API 密钥 SecretId";
            string secretKey = _configuration["secretKey"]; //"云 API 密钥 SecretKey";
            long durationSecond = 600;  //每次请求签名有效时长，单位为秒
            QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(
              secretId, secretKey, durationSecond);

            CosXml cosXml = new CosXmlServer(config, cosCredentialProvider);

            string Path = DateTime.Now.ToCstTime().ToString("yyyy-M-d")+"/";

            try
            {
                string bucket = _configuration["bucket"]; //存储桶，格式：BucketName-APPID
                string cosPath = Path; // 对象键
                PutObjectRequest putObjectRequest = new PutObjectRequest(bucket, cosPath, new byte[0]);

                cosXml.PutObject(putObjectRequest);
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
            }

            Path += ("B-" +  new Random().Next(1000000, 99999999).ToString() + new Random().Next(1000000, 99999999).ToString() + "." + fileAddName);
            try
            {
                string bucket = _configuration["bucket"]; //存储桶，格式：BucketName-APPID
                string key = Path; //对象键
                string srcPath = filePath;//本地文件绝对路径

                PutObjectRequest request = new PutObjectRequest(bucket, key, srcPath);
                //设置进度回调
                request.SetCosProgressCallback(delegate (long completed, long total)
                {
                    Console.WriteLine(String.Format("progress = {0:##.##}%", completed * 100.0 / total));
                });
                //执行请求
                PutObjectResult result = cosXml.PutObject(request);
                //对象的 eTag
                string eTag = result.eTag;
                return "//image-1256103450.cos.ap-guangzhou.myqcloud.com/"+Path;
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                Console.WriteLine("CosServerException: " + serverEx.GetInfo());
                return null;
            }
        }

        public async Task<bool> DeleteFieAsync(ApplicationUser user, string fileName)
        {
            //获取文件管理
            FileManager fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                return false;
            }
            else
            {
                UserFile userFile = fileManager.UserFiles.FirstOrDefault(s => s.FileName == fileName);
                if (userFile != null)
                {
                    DeleteFile(fileName);
                    fileManager.UserFiles.Remove(userFile);
                    fileManager.UsedSize -= (long)userFile.FileSize;
                    _fileManagerRepository.Update(fileManager);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteFile(string name)
        {
            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uploadsFolder = Path.Combine(uploadsFolder, name);
                File.Delete(uploadsFolder);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void CutImage(string imageName, int x, int y)
        {
            if (x != 0 && y != 0)
            {
                try
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string filePath = Path.Combine(uploadsFolder, imageName);
                    Bitmap source = new Bitmap(filePath);
                    int linshi_x, linshi_y;

                    linshi_x = source.Width;
                    linshi_y = (int)((double)linshi_x / x * y);
                    if (source.Height < linshi_y)
                    {
                        linshi_y = source.Height;
                        linshi_x = (int)((double)linshi_y / y * x);
                    }

                    RectangleF rectangleF = new RectangleF(0, 0, linshi_x, linshi_y);
                    Bitmap newBitmap = source.Clone(rectangleF, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    //保存裁剪的图片
                    newBitmap.Save(filePath);

                    newBitmap.Dispose();
                    source.Dispose();
                }
                catch
                {

                }

            }
        }

        /// 无损压缩图片  
        /// <param name="sFile">原图片</param>  
        /// <param name="dFile">压缩后保存位置</param>  
        /// <param name="dHeight">高度</param>  
        /// <param name="dWidth"></param>  
        /// <param name="flag">压缩质量(数字越小压缩率越高) 1-100</param>  
        /// <returns></returns>  
        public bool GetPicThumbnail(Stream sFile, string dFile, int dHeight, int dWidth, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromStream(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            int sW, sH;

            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);

            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();
            //以下代码为保存图片时，设置压缩质量  
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100  
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径  
                }
                else
                {
                    ob.Save(dFile, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }

        private static string GetFileBase64(string path)
        {
            FileStream fsForRead = new FileStream(path, FileMode.Open);
            string base64Str;
            try
            {
                //读入一个字节
                //读写指针移到距开头10个字节处
                fsForRead.Seek(0, SeekOrigin.Begin);
                byte[] bs = new byte[fsForRead.Length];
                int log = Convert.ToInt32(fsForRead.Length);
                //从文件中读取10个字节放到数组bs中
                fsForRead.Read(bs, 0, log);
                base64Str = Convert.ToBase64String(bs);

                return base64Str;
            }
            catch
            {
            }
            finally
            {
                fsForRead.Close();
            }
            return null;
        }

        public async Task<string> GetUserJWTokenAsync(ApplicationUser user)
        {
            var roles = await _signInManager.UserManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("image", GetImagePath(user.PhotoPath, "user.png")),
                new Claim("mbgimage", GetImagePath(user.MBgImage, "background.png")),
                new Claim("sbgimage", GetImagePath(user.SBgImage, "CnGal5thMin.png")),
                new Claim("userid",user.Id, "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.ToCstTime().AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task ArticleReaderNumUpAsync(long articleId)
        {
            await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == articleId).Set(s => s.ReaderCount, b => b.ReaderCount + 1).ExecuteAsync();

        }

        public async Task UpdateFavoritesCountAsync(long[] favoriteFolderIds)
        {
            foreach (var item in favoriteFolderIds)
            {
                var count = await _favoriteFolderRepository.GetAll().Include(s => s.FavoriteObjects).Where(s => s.Id == item).Select(s => s.FavoriteObjects.Count).FirstOrDefaultAsync();
                await _favoriteFolderRepository.GetRangeUpdateTable().Where(s => s.Id == item).Set(s => s.Count, b => count).ExecuteAsync();
            }
        }

        public async Task DeleteComment(long id)
        {
            Comment comment = await _commentRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == id);
            if (comment == null)
            {
                return;
            }
            //删除关联审核
            await _examineRepository.DeleteAsync(s => s.CommentId == id);
            //遍历删除子评论
            foreach (var item in comment.InverseParentCodeNavigation)
            {
                await DeleteComment(item.Id);
            }
            await _commentRepository.DeleteAsync(comment);
            return;
        }

        public async Task ExaminePublishCommentTextAsync(Comment comment, CommentText examine)
        {

            //复制基础数据
            comment.Text = examine.Text;
            comment.CommentTime = examine.CommentTime;
            comment.Type = examine.Type;
            comment.ApplicationUserId = examine.PubulicUserId;
            comment.Text = examine.Text;

            //查找父对象
            Article article = null;
            UserSpaceCommentManager userSpace = null;
            Comment replyComment = null;
            ApplicationUser userTemp = null;
            long tempId = 0;
            if (examine.Type != CommentType.CommentUser)
            {
                tempId = long.Parse(examine.ObjectId);
            }
            //判断当前是否能够编辑
            switch (examine.Type)
            {
                case CommentType.CommentArticle:
                    article = await _articleRepository.GetAll().Include(s => s.CreateUser).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (article == null || article.CanComment == false)
                    {
                        return;
                    }
                    break;
             
                case CommentType.CommentUser:
                    userTemp = await _userRepository.GetAll().Include(s => s.UserSpaceCommentManager).FirstOrDefaultAsync(s => s.Id == examine.ObjectId);
                    if (userTemp == null || userTemp.CanComment == false)
                    {
                        //判断是不是本人
                        if (examine.PubulicUserId != userTemp?.Id)
                        {
                            return;
                        }
                    }
                    if (userTemp.UserSpaceCommentManager == null)
                    {
                        userTemp.UserSpaceCommentManager = new UserSpaceCommentManager();
                        userTemp = await _userRepository.UpdateAsync(userTemp);
                    }
                    userSpace = userTemp.UserSpaceCommentManager;
                    break;
                case CommentType.ReplyComment:
                    replyComment = await _commentRepository.GetAll().Include(s => s.ApplicationUser).Include(s => s.Article).Include(s => s.UserSpaceCommentManager).FirstOrDefaultAsync(s => s.Id == tempId);
                    if (replyComment == null)
                    {
                        return;
                    }
                    break;
                default:
                    return;
            }


            //关联父对象
            if (examine.Type != CommentType.CommentUser)
            {
                tempId = long.Parse(examine.ObjectId);
            }
            switch (comment.Type)
            {
                case CommentType.CommentArticle:
                    comment.ArticleId = tempId;
                    break;
           
                case CommentType.CommentUser:
                    comment.UserSpaceCommentManager = userSpace;
                    comment.UserSpaceCommentManagerId = userSpace.Id;
                    break;
                case CommentType.ReplyComment:
                    //同步关联父对象的关联对象
                    comment.Article = replyComment.Article;
                    comment.UserSpaceCommentManager = replyComment.UserSpaceCommentManager;
                    comment.ParentCodeNavigation = replyComment;
                    break;
            }



            //保存
            comment = await _commentRepository.UpdateAsync(comment);
            //获取发表评论的用户
            ApplicationUser user = await _userManager.FindByIdAsync(examine.PubulicUserId);
            //向归属者发送消息
            Message message = null;
            switch (examine.Type)
            {
                case CommentType.CommentArticle:
                    if (examine.PubulicUserId == article.CreateUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的文章『" + (article.DisplayName ?? article.Name) + "』下回复了你『\n" + examine.Text + "\n』",
                        Link = "articles/index/" + article.Id,
                        LinkTitle = article.Name,
                        Type = MessageType.ArticleReply,
                        ApplicationUser = article.CreateUser,
                        ApplicationUserId = article.CreateUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.CommentUser:
                    if (user.Id == userTemp.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的空间下留言『\n" + examine.Text + "\n』",
                        Link = "space/index/" + userTemp.Id,
                        LinkTitle = userTemp.UserName,
                        Type = MessageType.SpaceReply,
                        ApplicationUser = userTemp,
                        ApplicationUserId = userTemp.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
                case CommentType.ReplyComment:
                    if (user.Id == replyComment.ApplicationUser.Id)
                    {
                        break;
                    }
                    message = new Message
                    {
                        Title = user.UserName,
                        PostTime = DateTime.Now.ToCstTime(),
                        Image = user.PhotoPath,
                        // Rank = "系统",
                        Text = "在你的评论『" + GetStringAbbreviation(replyComment.Text, 20) + "』下回复了你『\n" + examine.Text + "\n』",
                        Type = MessageType.CommentReply,
                        ApplicationUser = replyComment.ApplicationUser,
                        ApplicationUserId = replyComment.ApplicationUser.Id,
                        AdditionalInfor = comment.Id.ToString()
                    };
                    break;
            }
            if (message != null)
            {
                await _messageRepository.InsertAsync(message);

            } //缓存评论数
            int tempCount = 0;
            switch (comment.Type)
            {
                case CommentType.CommentArticle:
                    tempCount = await _commentRepository.CountAsync(s => s.ArticleId == tempId);
                    await _articleRepository.GetRangeUpdateTable().Where(s => s.Id == tempId).Set(s => s.CommentCount, b => tempCount).ExecuteAsync();
                    break;
            }
        }

        public async Task UniversalCommentExaminedAsync(Comment comment, ApplicationUser user, bool isAdmin, string examineStr, Operation operation, string note)
        {
            if (isAdmin)
            {
                //添加到审核列表
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = true,
                    Comment = comment,
                    PassedAdminName = user.UserName,
                    CommentId = comment.Id,
                    PassedTime = DateTime.Now.ToCstTime(),
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                await _examineRepository.InsertAsync(examine);
            }
            else
            {
                Examine examine = new Examine
                {
                    Operation = operation,
                    Context = examineStr,
                    IsPassed = null,
                    PassedTime = null,
                    Comment = comment,
                    CommentId = comment.Id,
                    ApplyTime = DateTime.Now.ToCstTime(),
                    ApplicationUserId = user.Id,
                    ApplicationUser = user,
                    Note = note
                };
                //添加到审核列表
                await _examineRepository.InsertAsync(examine);
            }
        }

        public async Task<bool> IsUserHavePermissionForCommmentAsync(long commentId, ApplicationUser user)
        {
            Comment comment = await _commentRepository.GetAll().Include(s => s.Article).Include(s => s.UserSpaceCommentManager).Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == commentId);
            if (comment != null)
            {
                //判断评论是否归属该用户
                if (comment.ApplicationUserId == user.Id)
                {

                }
                else if (comment.Article != null && comment.Article.CreateUserId == user.Id)
                {

                }
                else if (await _userManager.IsInRoleAsync(user, "Admin"))
                {

                }
                else if (comment.UserSpaceCommentManager != null && comment.UserSpaceCommentManager.ApplicationUserId == user.Id)
                {

                }
                else
                {
                    //不符合上述条件 返回
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> LoginHistoryUser(string name)
        {
            //查找历史用户 顺序为 登入Id 电子邮箱 昵称
            HistoryUser historyUser = await _historyUserRepository.FirstOrDefaultAsync(s => s.LoginName == name || s.Email == name || s.UserName == name);
            if (historyUser == null)
            {
                return null;
            }

            //通过后更新关联用户的密码
            ApplicationUser user = await _userManager.FindByNameAsync(historyUser.UserName);

            //获取长令牌
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            //获取短令牌
            await GetShortTokenAsync(token, user.UserName);
            //因为此处已经通过人机验证 直接获取验证码
            var result_2 = await SendVerificationEmailAsync(null, historyUser.Email, user.UserName);
            if (result_2 != null)
            {
                return null;
            }
            //删除历史用户
            // await _historyUserRepository.DeleteAsync(historyUser);
            return user.UserName;
        }

        /// <summary>
        /// 验证历史用户的密码是否正确
        /// </summary>
        /// <param name="inputPassword">当前用户输入的密码</param>
        /// <param name="historyPassword">历史单向加密后的密码</param>
        /// <param name="historyPasswordSalt">加密字符串</param>
        /// <returns></returns>
        public bool CheckHistoryUserPassword(string inputPassword, string historyPassword, string historyPasswordSalt)
        {
            string encoded = Encode(inputPassword, historyPasswordSalt);
            if (encoded == historyPassword)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// PHP sha1() 函数c#实现
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string PhpSha1(string data)
        {
            byte[] temp1 = Encoding.UTF8.GetBytes(data);
            SHA1CryptoServiceProvider sha = new();
            byte[] temp2 = sha.ComputeHash(temp1);
            sha.Clear();
            var output = BitConverter.ToString(temp2);
            output = output.Replace("-", "");
            output = output.ToLower();
            return output;
        }

        /// <summary>
        /// 单向加密明文密码
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private static string Encode(string plain, string salt)
        {
            //sha1(substr(sha1($plain),0,16) . substr(sha1($salt),0,16));
            return PhpSha1(string.Concat(PhpSha1(plain).AsSpan(0, 16), PhpSha1(salt).AsSpan(0, 16)));
        }


        public async Task<bool> IsExceedMaxErrorCount(string text, int limit, int maxMinutes)
        {
            DateTime timeNow = DateTime.Now.ToCstTime();
            var errors = await _errorCountRepository.GetAll().CountAsync(s => s.Text == text && s.LastUpdateTime.AddMinutes(maxMinutes) > timeNow);
            if (errors < limit)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task AddErrorCount(string text)
        {
            await _errorCountRepository.InsertAsync(new ErrorCount
            {
                LastUpdateTime = DateTime.Now.ToCstTime(),
                Text = text
            });
        }

        public async Task RemoveErrorCount(string text)
        {
            //查找是否存在计数器
            var error = await _errorCountRepository.FirstOrDefaultAsync(s => s.Text == text);
            if (error != null)
            {
                await _errorCountRepository.DeleteAsync(error);
                return;
            }
        }

        public async Task<bool> IsExceedMaxSendCount(string mail, int limit, int maxMinutes)
        {
            DateTime timeNow = DateTime.Now.ToCstTime();
            var errors = await _sendCountRepository.GetAll().CountAsync(s => s.Mail == mail && s.SendTime.AddMinutes(maxMinutes) > timeNow);
            if (errors < limit)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task AddSendCount(string mail, SendType type)
        {
            await _sendCountRepository.InsertAsync(new SendCount
            {
                SendTime = DateTime.Now.ToCstTime(),
                Type = type,
                Mail = mail
            });
        }

        public bool CheckRecaptcha(string token, string randstr, string ip)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = "SecretId",
                    SecretKey = "SecretKey"
                };

                ClientProfile clientProfile = new ClientProfile();
                HttpProfile httpProfile = new HttpProfile
                {
                    Endpoint = ("captcha.tencentcloudapi.com")
                };
                clientProfile.HttpProfile = httpProfile;

                CaptchaClient client = new CaptchaClient(cred, "", clientProfile);
                DescribeCaptchaResultRequest req = new DescribeCaptchaResultRequest
                {
                    CaptchaType = 9,
                    Ticket = token,
                    UserIp = ip,
                    Randstr = randstr,
                    CaptchaAppId = ulong.Parse(_configuration["TencentCaptchaAppId"]),
                    AppSecretKey = _configuration["TencentAppSecretKey"]
                };
                DescribeCaptchaResultResponse resp = client.DescribeCaptchaResultSync(req);

                if (resp.CaptchaCode == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public BootstrapBlazor.Components.ChartDataSource GetCountLine(Dictionary<string, List<CountLineModel>> data, string xString, string yString, string title)
        {
            var ds = new BootstrapBlazor.Components.ChartDataSource();
            ds.Options.X.Title = xString;
            ds.Options.Y.Title = yString;

            //查找最大的天数
            DateTime maxDay = DateTime.MinValue;
            DateTime minDay = DateTime.MaxValue;
            foreach (var item in data)
            {
                if (item.Value.Count == 0)
                {
                    continue;
                }
                var tempMaxDay = item.Value.Max(s => s.Time);
                if (tempMaxDay > maxDay)
                {
                    maxDay = tempMaxDay;
                }

                var TempMinDay = item.Value.Min(s => s.Time);
                if (TempMinDay < minDay)
                {
                    minDay = TempMinDay;
                }
            }


            List<string> labels = new List<string>();

            for (DateTime i = minDay; i <= maxDay; i = i.AddDays(1))
            {
                labels.Add(i.ToString("MM-dd"));
            }

            ds.Labels = labels;
            ds.Options.Title = title;

            foreach (var dataList in data)
            {
                if (dataList.Value.Count > 0)
                {
                    List<object> temp = new List<object>();
                    DateTime lastTime = dataList.Value.First().Time;

                    //给没有数据的天数添加默认值0
                    for (DateTime i = minDay; i < lastTime; i = i.AddDays(1))
                    {
                        temp.Add(0);
                    }
                    foreach (var item in dataList.Value)
                    {
                        //给没有数据的天数添加默认值0
                        for (DateTime i = lastTime.AddDays(1); i < item.Time; i = i.AddDays(1))
                        {
                            temp.Add(0);
                        }
                        //添加数据
                        temp.Add(item.Count);
                        lastTime = item.Time;
                    }
                    //给没有数据的天数添加默认值0
                    for (DateTime i = lastTime.AddDays(1); i <= maxDay; i = i.AddDays(1))
                    {
                        temp.Add(0);
                    }

                    ds.Data.Add(new BootstrapBlazor.Components.ChartDataset()
                    {
                        Label = dataList.Key,

                        Data = temp
                    });
                }
            }

            return ds;
        }


        public ArticleInforTipViewModel GetArticleInforTipViewModel(Article item)
        {
            return new ArticleInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Type = item.Type,
                DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                CreateUserName = item.CreateUser?.UserName,
                MainImage = GetImagePath(item.MainPicture, "certificate.png"),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount,
                ThumbsUpCount = item.ThumbsUpCount,
                CommentCount = item.CommentCount
            };
        }

        public async Task<List<ExaminedNormalListModel>> GetExaminesToNormalListAsync(IQueryable<Examine> examines)
        {
            var temp = await examines.AsNoTracking()
                          .Include(s => s.ApplicationUser)
                          .Include(s => s.Article)
                          .Select(n => new
                          {
                              n.Id,
                              n.ApplyTime,
                              n.PassedTime,
                              n.ArticleId,
                              n.CommentId,
                              ArticleName = n.Article == null ? "" : n.Article.Name,
                              UserId = n.ApplicationUserId,
                              n.ApplicationUser.UserName,
                              n.Operation,
                              n.IsPassed,
                          })
                          .ToListAsync();
            var result = new List<ExaminedNormalListModel>();
            foreach (var item in temp)
            {
                var tempModel = new ExaminedNormalListModel
                {
                    Id = item.Id,
                    ApplyTime = item.ApplyTime,
                    PassedTime = item.PassedTime,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Operation = item.Operation,
                    IsPassed = item.IsPassed
                };
                tempModel.RelatedId = item.ArticleId != null ? item.ArticleId.ToString() :  (item.CommentId != null ? item.CommentId.ToString() : "");
                tempModel.Type = item.ArticleId != null ? ExaminedNormalListModelType.Article : (item.CommentId != null ? ExaminedNormalListModelType.Comment :  ExaminedNormalListModelType.User);
                tempModel.RelatedName = item.ArticleId != null ? item.ArticleName : "";
                result.Add(tempModel);
            }

            return result;
        }

        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public string GetJsonStringView(string jsonStr)
        {
            if (jsonStr == null)
            {
                return "";
            }
            string resulte = "``` json\n" + ConvertJsonString(jsonStr) + "\n```";
            //使用markdown渲染
            return MarkdownToHtml(resulte);
        }

        public string MarkdownToHtml(string str)
        {
            if(str==null)
            {
                return "";
            }
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            return Markdown.ToHtml(str, pipeline);
        }

        public async Task UpdateUserIntegral(ApplicationUser user)
        {
            //计算积分
            try
            {

                user.DisplayIntegral = user.Integral;


                //计算贡献值
                user.DisplayLearningValue = user.LearningValue;

                await _userManager.UpdateAsync(user);
            }
            catch (Exception exc)
            {

            }
        }

        public async Task<string> GetUserLoginKeyAsync(string userId)
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            string keyStr = Guid.NewGuid().ToString();
            var key = await _loginkeyRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (key == null)
            {
                await _loginkeyRepository.InsertAsync(new Loginkey
                {
                    UserId = userId,
                    Key = keyStr,
                    CreateTime = tempDateTimeNow,
                });
            }
            else
            {
                key.CreateTime = tempDateTimeNow;
                key.Key = keyStr;
                await _loginkeyRepository.UpdateAsync(key);
            }
            return keyStr;
        }

        public async Task<string> GetUserFromLoginKeyAsync(string key)
        {
            DateTime tempDateTimeNow = DateTime.Now.ToCstTime();
            var result = await _loginkeyRepository.FirstOrDefaultAsync(s => s.Key == key && (s.CreateTime).AddHours(1) > tempDateTimeNow);
            if (result != null)
            {
                return result.UserId;
            }
            return null;
        }

        public async Task DeleteAllBackupInfor()
        {
            await _backUpArchiveDetailRepository.DeleteRangeAsync(s => true);
            await _backUpArchiveRepository.DeleteRangeAsync(s => true);
        }

        public async Task<string> CheckStringCompliance(string text, string ip)
        {
            try
            {
                //检查是否超过上限
                if (await IsExceedMaxErrorCount(ip, 5, 1))
                {
                    return "请求验证用户名合规次数达到上限";
                }

                //初始化
                var API_KEY = _configuration["BaiduAPIKey"];
                var SECRET_KEY = _configuration["BaiduSecretKey"];

                //获取令牌
                HttpClient client = _clientFactory.CreateClient();
                string jsonContent = await client.GetStringAsync("https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id=" + API_KEY + "&client_secret=" + SECRET_KEY + "&");
                JObject obj = JObject.Parse(jsonContent);
                var access_token = obj["access_token"].ToString();


                string token = access_token;
                string host = "https://aip.baidubce.com/rest/2.0/solution/v1/text_censor/v2/user_defined?access_token=" + token+"&text="+text;

                jsonContent = await client.GetStringAsync(host);
                obj = JObject.Parse(jsonContent);

                var conclusion = obj["conclusion"].ToString();



                //增加审核次数
                await AddErrorCount(ip);

                if (conclusion == "合规")
                {
                    return null;
                }
                else
                {
                    return "文本存在敏感词";
                }
            }
            catch(Exception exc)
            {
                return "请求审核文本失败";
            }

        }
    }
}
