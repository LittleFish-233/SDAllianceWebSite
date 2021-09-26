using Microsoft.AspNetCore.Components;
using SDAllianceWebSite.Shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDAllianceWebSite.Shared.Component.Others.Images
{
    public class ImagesLargeViewService
    {

        /// <summary>
        /// 获得 回调委托缓存集合
        /// </summary>
        private List<(IComponent Key, Func<ImagesLargeViewModel, ValueTask> Callback)> Cache { get; set; } = new();

        /// <summary>
        /// 设置当前页面 Title 方法
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public async ValueTask ViewLargeImages(ImagesLargeViewModel model)
        {
            var cb = Cache.FirstOrDefault().Callback;
            if (cb != null)
            {
                await cb.Invoke(model);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        internal void Register(IComponent key, Func<ImagesLargeViewModel, ValueTask> callback) => Cache.Add((key, callback));

        /// <summary>
        /// 注销事件
        /// </summary>
        internal void UnRegister(IComponent key)
        {
            var item = Cache.FirstOrDefault(i => i.Key == key);
            if (item.Key != null) Cache.Remove(item);
        }
    }
}
