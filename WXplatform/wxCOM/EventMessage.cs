using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wxCOM
{
    public class EventMessage:BaseMessage
    {
        public enum FuncFlag
        {
            None,
            Mark
        }

    }

    #region 事件类型枚举
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// 非事件类型
        /// </summary>
        NOEVENT,
        /// <summary>
        /// 订阅
        /// </summary>
        SUBSCRIBE,
        /// <summary>
        /// 取消订阅
        /// </summary>
        UNSUBSCRIBE,
        /// <summary>
        /// 扫描带参数的二维码
        /// </summary>
        SCAN,
        /// <summary>
        /// 地理位置
        /// </summary>
        LOCATION,
        /// <summary>
        /// 单击按钮
        /// </summary>
        CLICK,
        /// <summary>
        /// 链接按钮
        /// </summary>
        VIEW,
        /// <summary>
        /// 扫码推事件
        /// </summary>
        SCANCODE_PUSH,
        /// <summary>
        /// 扫码推事件且弹出“消息接收中”提示框
        /// </summary>
        SCANCODE_WAITMSG,
        /// <summary>
        /// 弹出系统拍照发图
        /// </summary>
        PIC_SYSPHOTO,
        /// <summary>
        /// 弹出拍照或者相册发图
        /// </summary>
        PIC_PHOTO_OR_ALBUM,
        /// <summary>
        /// 弹出微信相册发图器
        /// </summary>
        PIC_WEIXIN,
        /// <summary>
        /// 弹出地理位置选择器
        /// </summary>
        LOCATION_SELECT,
        /// <summary>
        /// 模板消息推送
        /// </summary>
        TEMPLATESENDJOBFINISH
    }
    #endregion

    #region 菜单扫描事件
    /// <summary>
    /// 菜单扫描事件
    /// </summary>
    public class ScanMenuEventMessage : EventMessage
    {

        /// <summary>
        /// 事件KEY值
        /// </summary>
        public string EventKey { get; set; }
        /// <summary>
        /// 扫码类型。qrcode是二维码，其他的是条码
        /// </summary>
        public string ScanType { get; set; }
        /// <summary>
        /// 扫描结果
        /// </summary>
        public string ScanResult { get; set; }
    }
    #endregion

    #region 普通菜单事件，包括click和view
    /// <summary>
    /// 普通菜单事件，包括click和view
    /// </summary>
    public class NormalMenuEventMessage : EventMessage
    {

        /// <summary>
        /// 事件KEY值，设置的跳转URL
        /// </summary>
        public string EventKey { get; set; }
  

    }
    #endregion

    #region 上报地理位置
    /// <summary>
    /// 上报地理位置实体
    /// </summary>
    public class LocationEventMessage : EventMessage
    {

        /// <summary>
        /// 地理位置纬度
        /// </summary>
        public string Latitude { get; set; }
        /// <summary>
        /// 地理位置经度
        /// </summary>
        public string Longitude { get; set; }
       /// <summary>
        /// 地理位置精度
       /// </summary>
        public string Precision { get; set; }

    }
    #endregion

    #region 扫描带参数的二维码实体
    /// <summary>
    /// 扫描带参数的二维码实体
    /// </summary>
    public class ScanEventMessage : EventMessage
    {

        /// <summary>
        /// 事件KEY值，是一个32位无符号整数，即创建二维码时的二维码scene_id
        /// </summary>
        public string EventKey { get; set; }
        /// <summary>
        /// 二维码的ticket，可用来换取二维码图片
        /// </summary>
        public string Ticket { get; set; }

    }
    #endregion

    #region 订阅/取消订阅事件
    /// <summary>
    /// 订阅/取消订阅事件
    /// </summary>
    public class SubEventMessage : EventMessage
    {
        private string _eventkey;
        /// <summary>
        /// 事件KEY值，qrscene_为前缀，后面为二维码的参数值（已去掉前缀，可以直接使用）
        /// </summary>
        public string EventKey
        {
            get { return _eventkey; }
            set { _eventkey = value.Replace("qrscene_", ""); }
        }
        /// <summary>
        /// 二维码的ticket，可用来换取二维码图片
        /// </summary>
        public string Ticket { get; set; }

    }
    #endregion
}