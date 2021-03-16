using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.IO;

namespace TestKernelServiceDesktop
{
    public static class DataModel
    {
        // 获得当前应用的用户目录
        public static string GetUserDirectory()
        {
            string product_name = "testKernelServiceDesktop";
            string userDir = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
product_name);
            PathUtil.TryCreateDir(userDir);
            return userDir;
        }

        // 获得 dp2kernel 模块的数据目录
        public static string GetKernelDataDirectory()
        {
            string result = Path.Combine(GetUserDirectory(), "kernel_data");
            PathUtil.TryCreateDir(result);
            return result;
        }
    }
}
