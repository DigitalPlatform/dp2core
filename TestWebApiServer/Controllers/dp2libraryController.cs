using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TestWebApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class dp2libraryController : ControllerBase
    {
        private readonly ILogger<dp2libraryController> _logger;

        public dp2libraryController(ILogger<dp2libraryController> logger)
        {
            _logger = logger;
        }

        [Route("Login")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(LoginResponseExamples))]
        [HttpPost]
        [HttpGet]
        public LoginResponse Login(
    string strUserName,
    string strPassword,
    string strParameters/*,
    out string strOutputUserName,
    out string strRights,
    out string strLibraryCode*/)
        {
            /*
            strOutputUserName = "";
            strRights = "";
            strLibraryCode = "";
            */

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
        }

        [Route("/Enum")]
        [HttpPost]
        [HttpGet]
        public ErrorCode Enum()
        {
            return ErrorCode.NoError;
        }
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
}
