using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;
using wxBIZ;

namespace wxCOM
{
    class Utils
    {
        #region 解析xml消息类型
        public static T ConvertObj<T>(string xmlstr)
        {
            XElement xdoc = XElement.Parse(xmlstr);
            var type = typeof(T);
            var t = Activator.CreateInstance<T>();
            foreach (XElement element in xdoc.Elements())
            {
                var pr = type.GetProperty(element.Name.ToString());
                if (element.HasElements)
                {//这里主要是兼容微信新添加的菜单类型。nnd，竟然有子属性，所以这里就做了个子属性的处理
                    foreach (var ele in element.Elements())
                    {
                        pr = type.GetProperty(ele.Name.ToString());
                        pr.SetValue(t, Convert.ChangeType(ele.Value, pr.PropertyType), null);
                    }
                    continue;
                }
                if (pr.PropertyType.Name == "MsgType")//获取消息模型
                {
                    pr.SetValue(t, (MsgType)Enum.Parse(typeof(MsgType), element.Value.ToUpper()), null);
                    continue;
                }
                if (pr.PropertyType.Name == "Event")//获取事件类型。
                {
                    pr.SetValue(t, (EventType)Enum.Parse(typeof(EventType), element.Value.ToUpper()), null);
                    continue;
                }
                pr.SetValue(t, Convert.ChangeType(element.Value, pr.PropertyType), null);
            }
            return t;
        }
        #endregion

        #region 回复消息
        /// <summary>
        /// 回复消息
        /// </summary>
        /// <param name="msg"></param>
        public static void ResponseWrite(string ToUserName, string strContent)
        {
            string responseContent = FormatTextXML(ToUserName, strContent);
            HttpContext.Current.Response.ContentType = "text/xml";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.Write(responseContent);
            HttpContext.Current.Response.End();
        }
        #endregion

        #region 返回图文消息项
        /// <summary>
        /// 返回图文消息项
        /// </summary>
        public static string Message_News_Item
        {
            get
            {
                return @"<item>
    <Title><![CDATA[{0}]]></Title> 
    <Description><![CDATA[{1}]]></Description>
    <PicUrl><![CDATA[{2}]]></PicUrl>
    <Url><![CDATA[{3}]]></Url>
    </item>";
            }
        }
        #endregion

        #region 返回格式化的Xml格式内容
        /// <summary>
        /// 返回格式化的Xml格式内容
        /// </summary>
        /// <param name="ToUserName">公众号</param>
        /// <param name="FromUserName">用户号</param>
        /// <param name="Content">回复内容</param>
        /// <returns></returns>
        private static string FormatTextXML(string ToUserName, string Content)
        {
            return "<xml><ToUserName><![CDATA[" + ToUserName + "]]></ToUserName><FromUserName><![CDATA[" + GetOpenID() + "]]></FromUserName><CreateTime>" + DateTime.Now.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds.ToString() + "</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[" + Content + "]]></Content><FuncFlag>1</FuncFlag></xml>";
        }
        #endregion

        #region 获取OpenID
        /// <summary>
        /// OpenID
        /// </summary>
        /// <returns></returns>
        private static string GetOpenID()
        {
            string openId = string.Empty;
            try
            {
                using(wxEntities context=new wxEntities())
                {
                   openId=context.T_Setting.Where(s => s.IsDeleted == false && s.SettingKey == "OpenID").FirstOrDefault().SettingValue;
                }
            }
            catch (Exception)
            {

            }
            return openId;
        }
        #endregion
    }
}
