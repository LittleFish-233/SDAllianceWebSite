using BootstrapBlazor.Components;
using Markdig;
using SDAllianceWebSite.Shared.Model;
using SDAllianceWebSite.Shared.ViewModel;
using SDAllianceWebSite.Shared.ViewModel.Accounts;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdown = Markdig.Markdown;

namespace SDAllianceWebSite.Shared.Helper
{
    public static class ToolHelper
    {
        //http://localhost:45160/
        //http://localhost:44360/
        //http://localhost:51313/
        //http://101.34.84.38:2001/
        //121.43.54.210
        //http://172.17.0.1:2001/
        //https://v3.cngal.org/


        public const string WebApiPath = "http://localhost:65160/";
        //public const string WebApiPath = "http://172.17.0.1:2001/";
        //public const string WebApiPath = "https://www.ruanmeng.love/";

        public static bool IsSSR => WebApiPath == "http://172.17.0.1:2001/";

        public static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public const int MaxEditorCount = 200;

        //临时储存的信息

        /// <summary>
        /// 身份验证成功后获得的标识 有效期一小时
        /// </summary>
        public static string LoginKey = string.Empty;
        /// <summary>
        /// 第三方登入成功 服务端也验证成功后 返回唯一标识 有效期一小时
        /// </summary>
        public static ThirdPartyLoginTempModel ThirdPartyLoginTempModel = null;

        public static string UserName = string.Empty;
        public static bool IsOnThirdPartyLogin = true;
        public static UserAuthenticationTypeModel UserAuthenticationTypeModel = new UserAuthenticationTypeModel();
    

        /// <summary>
        /// 获得枚举的displayName
        /// </summary>
        /// <param name="eum"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum eum)
        {
            var type = eum.GetType();//先获取这个枚举的类型
            var field = type.GetField(eum.ToString());//通过这个类型获取到值
            var obj = (DisplayAttribute)field.GetCustomAttribute(typeof(DisplayAttribute));//得到特性
            return obj.Name ?? "";
        }

        public static List<string> GetImageLinks(string context)
        {
            List<string> result = new List<string>();
            //查找符合markdown语法的图片
            string[] linshi = context.Split('!');
            for (int i = 1; i < linshi.Length; i++)
            {
                try
                {
                    string temp = MidStrEx(linshi[i], "[image](", ")");
                    if (string.IsNullOrWhiteSpace(temp) == false && temp.Contains("data:image") == false)
                    {
                        result.Add(temp);
                    }
                }
                catch
                {

                }

            }
            //查找符合html语法的图片
            linshi = context.Split("<img");
            if (linshi.Length > 1)
            {
                List<string> linshi2 = new List<string>();
                for (int i = 1; i < linshi.Length; i++)
                {
                    var linshi3 = linshi[i].Split(">");
                    if (linshi3.Length >= 1)
                    {
                        linshi2.Add(linshi3[0]);
                    }
                }
                //提取
                foreach (var item in linshi2)
                {
                    try
                    {
                        string temp = MidStrEx(item, "src=\"", "\"");
                        if (string.IsNullOrWhiteSpace(temp) == false && temp.Contains("data:image") == false)
                        {
                            result.Add(temp);
                        }
                    }
                    catch
                    {

                    }
                }

            }
            return result;
        }

        public static string MidStrEx(string sourse, string startstr, string endstr)
        {
            string result = string.Empty;
            int startindex, endindex;
            try
            {
                startindex = sourse.IndexOf(startstr);
                if (startindex == -1)
                    return result;
                string tmpstr = sourse[(startindex + startstr.Length)..];
                endindex = tmpstr.IndexOf(endstr);
                if (endindex == -1)
                    return result;
                result = tmpstr.Remove(endindex);
            }
            catch
            {
                //Log.WriteLog("MidStrEx Err:" + ex.Message);
            }
            return result;
        }

        public static string Base64EncodeName(string content)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return "A" + Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
        }

        public static string Base64DecodeName(string content)
        {
            content = content[1..];
            content = content.Replace("-", "+").Replace("_", "/");
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64EncodeUrl(string content)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return "A" + Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
        }

        public static string Base64DecodeUrl(string content)
        {
            content = content[1..];
            content = content.Replace("-", "+").Replace("_", "/").Replace("%3d", "=").Replace("%3D", "=");
            byte[] bytes = Convert.FromBase64String(content);
            string result = Encoding.UTF8.GetString(bytes);
            //判断Url是否是本地连接
            if (string.IsNullOrWhiteSpace(result))
            {
                return result;
            }
            else
            {
                if (result.StartsWith("/"))
                {
                    return result;
                }
                else
                {
                    if (result.StartsWith("https://app.ruanmeng.love")|| result.StartsWith("http://app.ruanmeng.love")||result.StartsWith("https://www.ruanmeng.love")|| result.StartsWith("http://www.ruanmeng.love") || result.StartsWith("https://localhost:") || result.StartsWith("http://localhost:"))
                    {
                        return result;
                    }
                    else
                    {
                        return "/";
                    }

                }
            }
        }

        public static string Base64EncodeString(string content)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return  Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
        }

        public static string Base64DecodeString(string content)
        {
            content = content.Replace("-", "+").Replace("_", "/");
            switch (content.Length % 4)
            {
                case 2: content += "=="; break;
                case 3: content += "="; break;
            }
            byte[] bytes = Convert.FromBase64String(content);
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="FileName">要计算 Sha1 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string ComputeFileSHA1(string FileName)
        {
            try
            {
                // 创建Hash算法对象
                byte[] hr; using (SHA1Managed Hash = new SHA1Managed()) // 创建文件流对象
                {
                    using FileStream fs = new FileStream(FileName, FileMode.Open);
                    hr = Hash.ComputeHash(fs);
                    // 计算
                }
                return BitConverter.ToString(hr).Replace("-", "");
                // 转化为十六进制字符串
            }
            catch (IOException) { return "Error:访问文件时出现异常"; }
        }
        /// <summary>
        /// 计算文件的 MD5 值
        /// </summary>
        /// <param name="FileName">要计算 Sha1 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static async Task<string> ComputeFileSHA1(Stream fs)
        {
            try
            {
                // 创建Hash算法对象
                byte[] hr;
                using (SHA1Managed Hash = new SHA1Managed()) // 创建文件流对象
                {

                    hr = await Hash.ComputeHashAsync(fs);
                    // 计算
                }
                return BitConverter.ToString(hr).Replace("-", "");
                // 转化为十六进制字符串
            }
            catch (IOException) { return "Error:访问文件时出现异常"; }
        }

        public static string GetFileBase64(Stream fsForRead)
        {
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
                return Guid.NewGuid().ToString();
            }
            finally
            {
                fsForRead.Close();
            }
        }
        /// <summary>
        /// 文件转为base64编码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static async Task<string> FileToBase64Str(Stream fsForRead)
        {
            string base64Str = string.Empty;
            try
            {

                byte[] bt = new byte[fsForRead.Length];

                //调用read读取方法

                await fsForRead.ReadAsync(bt.AsMemory(0, bt.Length));
                base64Str = Convert.ToBase64String(bt);
                fsForRead.Close();
                return base64Str;
            }
            catch
            {
                return base64Str;
            }
        }

        public static string GetFileBase64(string path)
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

       
        /// <summary>
        /// 检查手机号是否正确
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool CheckPhoneIsAble(string input)
        {
            if (input.Length < 11)
            {
                return false;
            }
            //电信手机号码正则
            string dianxin = @"^1[3578][01379]\d{8}$";
            Regex regexDX = new Regex(dianxin);
            //联通手机号码正则
            string liantong = @"^1[34578][01256]\d{8}";
            Regex regexLT = new Regex(liantong);
            //移动手机号码正则
            string yidong = @"^(1[012345678]\d{8}|1[345678][012356789]\d{8})$";
            Regex regexYD = new Regex(yidong);
            if (regexDX.IsMatch(input) || regexLT.IsMatch(input) || regexYD.IsMatch(input))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 时间戳转为C#格式时间10位
        /// </summary>
        /// <param name="curSeconds">Unix时间戳格式</param>
        /// <returns>C#格式时间</returns>
        public static DateTime GetDateTimeFrom1970Ticks(long curSeconds)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.AddSeconds(curSeconds);
        }

        /// <summary>
        /// 返回隐藏中间的字符串
        /// </summary>
        /// <param name="Input">输入</param>
        /// <returns>输出</returns>
        public static string GetxxxString(string Input)
        {
            if(string.IsNullOrEmpty(Input))
            {
                return "";
            }
            int length = Input.Length / 2;
            string Output = "";
            switch (Input.Length)
            {
                case 1:
                    Output = "*";
                    break;
                case 2:
                    Output = Input[0] + "*";
                    break;
                case 0:
                    Output = "";
                    break;
                default:
                    Output = Input.Substring(0, length/2);
                    for (int i = 0; i < ((double)Input.Length / 2); i++)
                    {
                        Output += "*";
                    }
                    Output += Input.Substring(Input.Length - length/2, length/2);
                    break;
            }
            return Output;
        }

        public static string GetThirdPartyLoginUrl(string returnUrl,ThirdPartyLoginType type)
        {
            string callbackUrl = GetThirdPartyCallbackUrl(type);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = Base64EncodeUrl("/");
            }

            switch (type)
            {
                case ThirdPartyLoginType.Microsoft:
                    return "https://login.microsoftonline.com/5486ee26-7c6a-4246-8776-1f0d104f861a/oauth2/authorize?response_type=code&redirect_uri=" + callbackUrl + "&state=" + returnUrl + "&client_id=a579e682-d4fc-45ff-a77d-96883700df69";
                case ThirdPartyLoginType.GitHub:
                    return "https://github.com/login/oauth/authorize?client_id=279b4ff2b18e5db533c5&redirect_uri=" + callbackUrl + "&state=" + returnUrl;
                case ThirdPartyLoginType.Gitee:
                    return "https://gitee.com/oauth/authorize?client_id=3af7e88727202f90fcb2531b0d808a1c0bbd2df887167615c395b7420f9cde23&redirect_uri=" + callbackUrl + "&response_type=code&state=" + returnUrl;
            }

            return null;
        }

        public static string GetThirdPartyCallbackUrl(ThirdPartyLoginType type)
        {
            string callbackUrl = IsSSR ? "https://www.ruanmeng.love/account/" : "https://app.ruanmeng.love/account/";
            //string callbackUrl = "http://localhost:3001/account/";
            switch (type)
            {
                case ThirdPartyLoginType.Microsoft:
                    callbackUrl += "microsoftlogin/";
                    return callbackUrl;
                case ThirdPartyLoginType.GitHub:
                    callbackUrl += "githublogin/";
                    return callbackUrl;
                case ThirdPartyLoginType.Gitee:
                    callbackUrl += "giteelogin/";
                    return callbackUrl;
            }

            return null;
        }
    }


}
