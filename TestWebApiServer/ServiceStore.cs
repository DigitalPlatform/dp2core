using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dp2.LibraryService;

namespace TestWebApiServer
{
    public static class ServiceStore
    {
        static Hashtable _serviceTable = new Hashtable();

        public static LibraryService GetService(string instance_name, 
            string session_id)
        {
            string key = instance_name + "_" + session_id;
            LibraryService service = (LibraryService)_serviceTable[key];
            if (service == null)
            {
                var instance = InstanceCollection.FindInstance(instance_name);
                service = new LibraryService(instance.Application, 
                    new DigitalPlatform.LibraryServer.SessionInfo(instance.Application));
                _serviceTable[key] = service;
            }

            return service;
        }
    }
}
