using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dp2.LibraryService;
using Microsoft.AspNetCore.Http;

namespace dp2LibraryServer
{
    public static class ServiceStore
    {
        static Hashtable _serviceTable = null;  // new Hashtable();

        public static LibraryService GetService(
            HttpContext context,
            bool useSession = true)
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;

            string instance_name = (string)context.Request.RouteValues["instance"];
            string session_id = context.Session.Id;

            // 必须要用 Session 放一点东西，session id 才能持久
            if (useSession)
            {
                int? used = context.Session.GetInt32("used");
                if (used == null)
                    context.Session.SetInt32("used", 1);
            }

            if (_serviceTable == null)
            {
                var instance = InstanceCollection.FindInstance(instance_name);
                return new LibraryService(instance.Application,
                    context);
            }

            string key = instance_name + "_" + session_id;
            LibraryService service = (LibraryService)_serviceTable[key];
            if (service == null)
            {
                var instance = InstanceCollection.FindInstance(instance_name);
                service = new LibraryService(instance.Application,
                    context);
                _serviceTable[key] = service;
            }

            return service;
        }

        /*
        public static bool DeleteService(string instance_name,
            string session_id)
        {
            string key = instance_name + "_" + session_id;
            LibraryService service = (LibraryService)_serviceTable[key];
            if (service == null)
                return false;
            _serviceTable.Remove(key);

            service.Dispose();
            // TODO: 记得释放 SessionInfo 和 dp2kernel 的 Session
        }
        */
    }
}
