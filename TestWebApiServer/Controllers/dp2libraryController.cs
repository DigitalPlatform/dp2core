using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalPlatform.LibraryServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

namespace TestWebApiServer.Controllers
{
    [ApiController]
    [Route("[controller]/{instance}/[action]")]
    [Route("[controller]/[action]")]
    public class dp2libraryController : ControllerBase
    {
        private readonly ILogger<dp2libraryController> _logger;

        public dp2libraryController(ILogger<dp2libraryController> logger)
        {
            _logger = logger;
        }

        // [Route("[action]")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(LoginResponseExamples))]
        [HttpPost]
        [HttpGet]
        public LoginResponse Login(
            [FromBody] LoginRequest request
/*
[FromForm] string strUserName,
[FromForm] string strPassword,
[FromForm] string strParameters*//*,
out string strOutputUserName,
out string strRights,
out string strLibraryCode*/)
        {
            /*
            strOutputUserName = "";
            strRights = "";
            strLibraryCode = "";
            */

            // 获得实例名
            // var instance = (string)HttpContext.Request.RouteValues["instance"];

            using (var service = ServiceStore.GetService(HttpContext))
            {
                /*
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-5.0
                SessionInfo info = (SessionInfo)HttpContext.Session.Get<SessionInfo>("session");
                if (info == null)
                {
                    info = new SessionInfo
                    {
                        UserName = strUserName,
                        Password = strPassword
                    };
                    HttpContext.Session.Set<SessionInfo>("session", info);
                }
                */
                var result = service.Login(request.strUserName,
                    request.strPassword,
                    request.strParameters,
                    out string strOutputUserName,
                    out string strRights,
                    out string strLibraryCode);
                return new LoginResponse
                {
                    LoginResult = result,
                    strOutputUserName = strOutputUserName,
                    strRights = strRights,
                    strLibraryCode = strLibraryCode
                };
            }

            /*
            return new LoginResponse
            {
                LoginResult = new LibraryServerResult
                {
                    Value = 0,
                    ErrorInfo = "errorInfo",
                    ErrorCode = ErrorCode.AccessDenied
                },
                strOutputUserName = "outputUserName",
                strRights = "rights",
                strLibraryCode = "libraryCode"
            };
            */
        }

        [HttpPost]
        [HttpGet]
        public LogoutResponse Logout()
        {
            /*
            // 获得实例名
            var instance = (string)HttpContext.Request.RouteValues["instance"];

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            */
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.Logout();
                return new LogoutResponse { LogoutResult = result };
            }
        }

        // [Route("/Enum")]
        [HttpPost]
        [HttpGet]
        public ErrorCode Enum()
        {
            return ErrorCode.NoError;
        }

        // [Route("/Result")]
        [HttpPost]
        [HttpGet]
        public LibraryServerResult Result()
        {
            return new LibraryServerResult
            {
                Value = -1,
                ErrorInfo = "error",
                ErrorCode = ErrorCode.AccessDenied
            };
        }
    }

    public class LoginRequest
    {
        public string strUserName { get; set; }
        public string strPassword { get; set; }
        public string strParameters { get; set; }
    }

    public class LoginResponse
    {
        public LibraryServerResult LoginResult { get; set; }
        /// <summary>
        /// Output UserName
        /// </summary>
        /// <example>userName</example>
        public string strOutputUserName { get; set; }
        /// <summary>
        /// Rights
        /// </summary>
        /// <example>search</example>
        public string strRights { get; set; }
        /// <summary>
        /// Library Code
        /// </summary>
        /// <example></example>
        public string strLibraryCode { get; set; }
    }


    public class LogoutResponse
    {
        public LibraryServerResult LogoutResult { get; set; }
    }

    public class LoginResponseExamples : IExamplesProvider<LoginResponse>
    {
        public LoginResponse GetExamples()
        {
            return new LoginResponse
            {
                LoginResult = new LibraryServerResult
                {
                    Value = 0,
                    ErrorInfo = "errorInfo",
                    ErrorCode = ErrorCode.AccessDenied
                },
                strOutputUserName = "outputUserName1",
                strRights = "rights1",
                strLibraryCode = "libraryCode1"
            };
        }
    }

    /*
public class LibraryServerResultExamples : IExamplesProvider<LibraryServerResult>
{
    public LibraryServerResult GetExamples()
    {
        return new LibraryServerResult
        {
            Value = 0,
            ErrorInfo = "errorInfo",
            ErrorCode = ErrorCode.AccessDenied
        };
    }
}
*/

    /*
    public class SessionInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool HasLogin { get; set; }

    }

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
    */
}
