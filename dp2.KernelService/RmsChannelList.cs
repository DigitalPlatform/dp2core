using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Core;
using Microsoft.AspNetCore.Http;

namespace dp2.KernelService
{
    /// <summary>
    /// 用于集中管理正在请求中的 RmsChannel 的集合。以方便 dp2library 退出时候迅速终止每个通道的请求
    /// </summary>
    public class RmsChannelList : SafeList<RmsChannel>
    {
        bool _disabled = false; // 是否处在禁用状态

        public bool Disabled
        {
            get
            {
                return _disabled;
            }
            set
            {
                _disabled = value;
            }
        }

        public void Abort()
        {
            foreach (RmsChannel channel in this)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public RmsChannel GetChannel(
            RmsChannelCollection channels,
            HttpContext context,
            string strServerUrl)
        {
            if (_disabled)
                return null;

            // TODO: 没法中途中断
            RmsChannel channel = channels.GetChannel(strServerUrl, context);
            this.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(
            RmsChannelCollection channels,
            RmsChannel channel)
        {
            // channels.ReturnChannel(channel);
            this.Remove(channel);
        }
    }

}
