using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wxCOM
{
    public class WXApiUrl
    {
        #region 微信api 地址
        /// <summary>
        /// 微信api 调用地址
        /// </summary>
        public Dictionary<string, string> Dic_WXUrls = new Dictionary<string, string> { 
            {"Aauth2CheckAccessTokenUrl","https://api.weixin.qq.com/sns/auth?access_token={0}&openid={1}"},
            {"Aauth2Confirm","https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect"},
            {"Aauth2GetAccessTokenUrl","https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code"},
            {"Aauth2GetWechatUserInfoUrl","https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN"},
            {"Aauth2RefreshAccessTokenUrl","https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type=refresh_token&refresh_token={1}"},
            {"JsapiTicketUrl","https://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token={0}"},
            {"UnifiedorderUrl","https://api.mch.weixin.qq.com/pay/unifiedorder"},
            //创建自定义菜单 Post
            {"CreateWxMenu","https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}"},
            //查询自定义菜单 get方法
            {"QueryWxMenu","https://api.weixin.qq.com/cgi-bin/menu/get?access_token=ACCESS_TOKEN"},
            //删除自定义菜单 GET
            {"DeleteWxMenu","https://api.weixin.qq.com/cgi-bin/menu/delete?access_token=ACCESS_TOKEN"},
            //获取Access_token  GET
            {"GetAccess_token","https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}"},
        };
        #endregion
    }
}
