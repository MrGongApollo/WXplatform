using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using wxBIZ;
using wxCOM;

namespace WXplatform.Areas.weixin.Help
{
    public class CommonHelper
    {
        #region 操作类型枚举
        /// <summary>
        /// 操作类型枚举
        /// </summary>
        public enum OperateType
        {
            /// <summary>
            /// 查看
            /// </summary>
            view,
            /// <summary>
            /// 登录
            /// </summary>
            login,
            /// <summary>
            /// 新增
            /// </summary>
            add,
            /// <summary>
            /// 修改
            /// </summary>
            modify,
            /// <summary>
            /// 删除
            /// </summary>
            delete
        }
        #endregion

        #region 记录日志
        #region 记录日志到数据库（用户日志表）
        /// <summary>
        /// 记录日志到数据库
        /// </summary>
        /// <param name="strTxt">内容</param>
        public void WriteLogToDB(string strTxt, OperateType Otp, string Ip)
        {
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    T_logs _log = context.T_logs.Create();
                    _log.LogId = Guid.NewGuid().ToString("N");
                    _log.OperateContent = strTxt;
                    _log.CreateTime = DateTime.Now;
                    _log.OperateType = Otp.ToString();
                    _log.UserIP = Ip;
                    context.T_logs.Add(_log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }



        #endregion
        #region 记录日志到数据库（系统日志）
        /// <summary>
        /// 记录系统日志
        /// </summary>
        public void WriteSysLogToDB(string strContent)
        {
            try
            {
                using (wxEntities context = new wxEntities())
                {
                    T_SysLogs _log = context.T_SysLogs.Create();
                    _log.SysLogId = Guid.NewGuid().ToString("N");
                    _log.SysContent = strContent;
                    _log.CreateTime = DateTime.Now;
                    context.T_SysLogs.Add(_log);
                    context.SaveChanges();
                }
            }
            catch
            {
            }
        }
        #endregion
        #region 记录错误日志到本地
        /// <summary>
        /// 记录系统日志
        /// </summary>
        public void WriteSysLogToLocal(string exmsg)
        {
            try
            {
                using (FileStream sw = new FileStream("err.txt", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    char[] charData = exmsg.ToCharArray();
                    //初始化字节数组
                    byte[] byData = new byte[charData.Length];
                    //将字符数组转换为正确的字节格式
                    Encoder enc = Encoding.UTF8.GetEncoder();
                    enc.GetBytes(charData, 0, charData.Length, byData, 0, true);
                    sw.Seek(0, SeekOrigin.Begin);
                    sw.Write(byData, 0, byData.Length);
                }
            }
            catch
            {
            }
        }
        #endregion
        #endregion
               
        #region 发送消息
        /// <summary>
        /// 发送文字消息
        /// </summary>
        /// <param name="Message">消息内容</param>
        /// <param name="fakeid">用户fakeid</param>
        /// <returns></returns>        
        public bool SendMsgText(string Message, string fakeid)
        {
            string _token = new Utils().GetAccessToken();
            bool result = false;
            string strMsg = System.Web.HttpUtility.UrlEncode(Message);
            string padate = "type=1&content=" + strMsg + "&error=false&tofakeid=" + fakeid + "&token=" + _token + "&ajax=1";
            string url = "https://mp.weixin.qq.com/cgi-bin/singlesend?t=ajax-response&lang=zh_CN";
            byte[] byteArray = Encoding.UTF8.GetBytes(padate); // 转化
            HttpWebRequest webRequest2 = (HttpWebRequest)WebRequest.Create(url);
            webRequest2.Referer = "https://mp.weixin.qq.com/cgi-bin/singlesendpage?t=message/send&action=index&tofakeid=" + fakeid + "&token=" + _token + "&lang=zh_CN";
            webRequest2.Method = "POST";
            webRequest2.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1";
            webRequest2.ContentType = "application/x-www-form-urlencoded";
            webRequest2.ContentLength = byteArray.Length;
            Stream newStream = webRequest2.GetRequestStream();
            // Send the data. 
            newStream.Write(byteArray, 0, byteArray.Length); //写入参数 
            newStream.Close();
            HttpWebResponse response2 = (HttpWebResponse)webRequest2.GetResponse();
            StreamReader sr2 = new StreamReader(response2.GetResponseStream(), Encoding.Default);
            string text2 = sr2.ReadToEnd();
            if (text2.Contains("ok") || text2.Contains("10706") || text2.Contains("10703"))
            {
                result = true;
            }
            return result;
        }
        #endregion
        
        #region SHA1加密
        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public string SHA1(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return txt;
            }
            byte[] cleanBytes = Encoding.Default.GetBytes(txt);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.Create().ComputeHash(cleanBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "");
        }
        #endregion

        #region 返回的JSON处理字符串
        /// <summary>  
        /// 返回JSon数据  
        /// </summary>  
        /// <param name="JSONData">要处理的JSON数据</param>  
        /// <param name="Url">要提交的URL</param>  
        /// <returns>返回的JSON处理字符串</returns>  
        public string GetResponseData(string JSONData, string Url)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JSONData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            Stream reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);

            //声明一个HttpWebRequest请求  
            request.Timeout = 90000;
            //设置连接超时时间  
            request.Headers.Set("Pragma", "no-cache");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamReceive = response.GetResponseStream();
            Encoding encoding = Encoding.UTF8;

            StreamReader streamReader = new StreamReader(streamReceive, encoding);
            string strResult = streamReader.ReadToEnd();
            streamReceive.Dispose();
            streamReader.Dispose();

            return strResult;
        }
        #endregion
    }

}