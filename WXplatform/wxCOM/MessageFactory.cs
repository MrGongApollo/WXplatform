﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace wxCOM
{
    public class MessageFactory
    {
        private static List<BaseMsg> _queue;
        public static BaseMessage CreateMessage(string xml)
        {
            if (_queue == null)
            {
                _queue = new List<BaseMsg>();
            }
            else if (_queue.Count >= 50)
            {
                _queue = _queue.Where(q => { return q.CreateTime.AddSeconds(20) > DateTime.Now; }).ToList();//保留20秒内未响应的消息
            }
            XElement xdoc = XElement.Parse(xml);
            var msgtype = xdoc.Element("MsgType").Value.ToUpper();
            var FromUserName = xdoc.Element("FromUserName").Value;
            var MsgId = xdoc.Element("MsgId").Value;
            var CreateTime = xdoc.Element("CreateTime").Value;
            MsgType type = (MsgType)Enum.Parse(typeof(MsgType), msgtype);
            if (type != MsgType.EVENT)
            {
                if (_queue.FirstOrDefault(m => { return m.MsgFlag == MsgId; }) == null)
                {
                    _queue.Add(new BaseMsg
                    {
                        CreateTime = DateTime.Now,
                        FromUser = FromUserName,
                        MsgFlag = MsgId
                    });
                }
                else
                {
                    return null;
                }

            }
            else
            {
                if (_queue.FirstOrDefault(m => { return m.MsgFlag == CreateTime; }) == null)
                {
                    _queue.Add(new BaseMsg
                    {
                        CreateTime = DateTime.Now,
                        FromUser = FromUserName,
                        MsgFlag = CreateTime
                    });
                }
                else
                {
                    return null;
                }
            }
            switch (type)
            {
                case MsgType.TEXT: return Utils.ConvertObj<TextMessage>(xml);
                case MsgType.IMAGE: return Utils.ConvertObj<ImgMessage>(xml);
                case MsgType.VIDEO: return Utils.ConvertObj<VideoMessage>(xml);
                case MsgType.VOICE: return Utils.ConvertObj<VoiceMessage>(xml);
                case MsgType.LINK:
                    return Utils.ConvertObj<LinkMessage>(xml);
                case MsgType.LOCATION:
                    return Utils.ConvertObj<LocationMessage>(xml);
                case MsgType.EVENT://事件类型
                    {
                        var eventtype = (EventType)Enum.Parse(typeof(EventType), xdoc.Element("Event").Value.ToUpper());
                        switch (eventtype)
                        {
                            case EventType.CLICK:
                                return Utils.ConvertObj<NormalMenuEventMessage>(xml);
                            case EventType.VIEW: return Utils.ConvertObj<NormalMenuEventMessage>(xml);
                            case EventType.LOCATION: return Utils.ConvertObj<LocationEventMessage>(xml);
                            //case EventType.LOCATION_SELECT: return Utils.ConvertObj<LocationMenuEventMessage>(xml);
                            case EventType.SCAN: return Utils.ConvertObj<ScanEventMessage>(xml);
                            case EventType.SUBSCRIBE: return Utils.ConvertObj<SubEventMessage>(xml);
                            case EventType.UNSUBSCRIBE: return Utils.ConvertObj<SubEventMessage>(xml);
                            case EventType.SCANCODE_WAITMSG: return Utils.ConvertObj<ScanMenuEventMessage>(xml);
                            default:
                                return Utils.ConvertObj<EventMessage>(xml);
                        }
                    } break;
                default:
                    return Utils.ConvertObj<BaseMessage>(xml);
            }
        }
    }
}