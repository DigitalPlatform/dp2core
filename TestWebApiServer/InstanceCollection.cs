using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DigitalPlatform.LibraryServer;
using dp2.LibraryService;
using dp2Kernel;

namespace TestWebApiServer
{
    public static class InstanceCollection
    {
        static List<Instance> _instances = new List<Instance>();

        public static void Initialize(string directory)
        {
            _instances.Clear();

            DirectoryInfo di = new DirectoryInfo(directory);
            var subs = di.GetDirectories();
            foreach (var sub in subs)
            {
                string library_data_dir = Path.Combine(sub.FullName, "library_data");
                string kernel_data_dir = Path.Combine(sub.FullName, "kernel_data");

                var kernel = KernelService.NewApplication(kernel_data_dir);
                var app = LibraryService.NewApplication(library_data_dir, kernel);
                _instances.Add(new Instance
                {
                    Name = sub.Name,
                    Application = app
                });
            }
        }

        public static Instance FindInstance(string instance)
        {
            if (string.IsNullOrEmpty(instance))
                return _instances[0];
            return _instances.Find(o => o.Name == instance);
        }
    }

    public class Instance
    {
        public string Name { get; set; }
        public LibraryApplication Application { get; set; }
    }
}
