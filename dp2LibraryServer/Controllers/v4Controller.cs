using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.rms;

namespace dp2LibraryServer.Controllers
{

    [ApiController]
    [ApiVersion("4.0")]
    [ApiExplorerSettings(GroupName = "v4")]
    //[Route("dp2library/v{version:apiVersion}/{instance}/[action]")]
    //[Route("dp2library/v{version:apiVersion}/[action]")]
    [Route("dp2library/v4/{instance}/[action]")]
    [Route("dp2library/v4/[action]")]
    public class v4Controller : ControllerBase
    {
        private readonly ILogger<v3Controller> _logger;

        public v4Controller(ILogger<v3Controller> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [HttpGet]
        public GetVersionResponse GetVersion()
        {
            using (var service = ServiceStore.GetService(HttpContext, false))
            {
                var result = service.GetVersion(out string uid);
                return new GetVersionResponse
                {
                    GetVersionResult = result,
                    uid = uid
                };
            }
        }

        #region GetVersion()

        public class GetVersionResponse
        {
            public LibraryServerResult GetVersionResult { get; set; }
            public string uid { get; set; }
        }

        #endregion

        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(LoginResponseExamples))]
        [HttpPost]
        public LoginResponse Login([FromBody] LoginRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
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
        }

        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(LoginResponseExamples))]
        [HttpGet]
        public LoginResponse Login(string strUserName,
            string strPassword,
            string strParameters)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.Login(strUserName,
                    strPassword,
                    strParameters,
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
        }

        #region Login()

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

        #endregion

        [HttpPost]
        [HttpGet]
        public LogoutResponse Logout()
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.Logout();
                return new LogoutResponse { LogoutResult = result };
            }
        }

        #region Logout()

        public class LogoutResponse
        {
            public LibraryServerResult LogoutResult { get; set; }
        }

        #endregion

        [HttpPost]
        public SetLangResponse SetLang([FromBody] SetLangRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SetLang(request.strLang, out string strOldLang);
                return new SetLangResponse
                {
                    SetLangResult = result,
                    strOldLang = strOldLang
                };
            }
        }

        [HttpGet]
        public SetLangResponse SetLang(string strLang)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SetLang(strLang, out string strOldLang);
                return new SetLangResponse
                {
                    SetLangResult = result,
                    strOldLang = strOldLang
                };
            }
        }

        #region SetLang()

        public class SetLangResponse
        {
            public LibraryServerResult SetLangResult { get; set; }
            public string strOldLang { get; set; }
        }

        public class SetLangRequest
        {
            public string strLang { get; set; }
        }

        #endregion

        // 停止检索
        [HttpPost]
        [HttpGet]
        public void Stop()
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                service.Stop();
            }
        }

        // 校验读者密码
        [HttpPost]
        public VerifyReaderPasswordResponse VerifyReaderPassword([FromBody] VerifyReaderPasswordRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.VerifyReaderPassword(request.strReaderBarcode, request.strReaderPassword);
                return new VerifyReaderPasswordResponse { VerifyReaderPasswordResult = result };
            }
        }

        [HttpGet]
        public VerifyReaderPasswordResponse VerifyReaderPassword(string strReaderBarcode,
            string strReaderPassword)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.VerifyReaderPassword(strReaderBarcode,
                    strReaderPassword);
                return new VerifyReaderPasswordResponse { VerifyReaderPasswordResult = result };
            }
        }

        #region VerifyReaderPassword()

        public class VerifyReaderPasswordResponse
        {
            public LibraryServerResult VerifyReaderPasswordResult { get; set; }
        }

        public class VerifyReaderPasswordRequest
        {
            public string strReaderBarcode { get; set; }
            public string strReaderPassword { get; set; }
        }

        #endregion

        // 修改读者密码
        [HttpPost]
        public ChangeReaderPasswordResponse ChangeReaderPassword(
            [FromBody] ChangeReaderPasswordRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.ChangeReaderPassword(request.strReaderBarcode,
                    request.strReaderOldPassword,
                    request.strReaderNewPassword);
                return new ChangeReaderPasswordResponse { ChangeReaderPasswordResult = result };
            }
        }

        [HttpGet]
        public ChangeReaderPasswordResponse ChangeReaderPassword(
    string strReaderBarcode,
    string strReaderOldPassword,
    string strReaderNewPassword)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.ChangeReaderPassword(strReaderBarcode,
                    strReaderOldPassword,
                    strReaderNewPassword);
                return new ChangeReaderPasswordResponse { ChangeReaderPasswordResult = result };
            }
        }

        #region ChangeReaderPassword()

        public class ChangeReaderPasswordResponse
        {
            public LibraryServerResult ChangeReaderPasswordResult { get; set; }
        }

        public class ChangeReaderPasswordRequest
        {
            public string strReaderBarcode { get; set; }
            public string strReaderOldPassword { get; set; }
            public string strReaderNewPassword { get; set; }
        }

        #endregion

        // 获得读者记录
        [HttpPost]
        public GetReaderInfoResponse GetReaderInfo(
            [FromBody] GetReaderInfoRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.GetReaderInfo(
                    request.strBarcode,
                    request.strResultTypeList,
                    out string[] results,
                    out string strRecPath,
                    out byte[] baTimestamp);
                return new GetReaderInfoResponse
                {
                    GetReaderInfoResult = result,
                    results = results,
                    strRecPath = strRecPath,
                    baTimestamp = baTimestamp
                };
            }
        }

        [HttpGet]
        public GetReaderInfoResponse GetReaderInfo(
string strBarcode,
string strResultTypeList)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.GetReaderInfo(strBarcode,
                    strResultTypeList,
                    out string[] results,
                    out string strRecPath,
                    out byte[] baTimestamp);
                return new GetReaderInfoResponse
                {
                    GetReaderInfoResult = result,
                    results = results,
                    strRecPath = strRecPath,
                    baTimestamp = baTimestamp
                };
            }
        }

        #region GetReaderInfo()

        public class GetReaderInfoResponse
        {
            public LibraryServerResult GetReaderInfoResult { get; set; }
            public string[] results { get; set; }
            public string strRecPath { get; set; }

            // 注: 这里会用到 base64 string 输出 JSON
            public byte[] baTimestamp { get; set; }
        }

        public class GetReaderInfoRequest
        {
            public string strBarcode { get; set; }
            public string strResultTypeList { get; set; }
        }

        #endregion

        [HttpPost]
        public SetReaderInfoResponse SetReaderInfo(
            [FromBody] SetReaderInfoRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SetReaderInfo(
                    request.strAction,
    request.strRecPath,
    request.strNewXml,
    request.strOldXml,
    request.baOldTimestamp,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedRecPath,
    out byte[] baNewTimestamp,
    out ErrorCodeValue kernel_errorcode);
                return new SetReaderInfoResponse
                {
                    SetReaderInfoResult = result,
                    strExistingXml = strExistingXml,
                    strSavedXml = strSavedXml,
                    strSavedRecPath = strSavedRecPath,
                    baNewTimestamp = baNewTimestamp
                };
            }
        }

        // 设置读者记录
        [HttpGet]
        public SetReaderInfoResponse SetReaderInfo(
    string strAction,
    string strRecPath,
    string strNewXml,
    string strOldXml,
    byte[] baOldTimestamp)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SetReaderInfo(
                    strAction,
    strRecPath,
    strNewXml,
    strOldXml,
    baOldTimestamp,
    out string strExistingXml,
    out string strSavedXml,
    out string strSavedRecPath,
    out byte[] baNewTimestamp,
    out ErrorCodeValue kernel_errorcode);
                return new SetReaderInfoResponse
                {
                    SetReaderInfoResult = result,
                    strExistingXml = strExistingXml,
                    strSavedXml = strSavedXml,
                    strSavedRecPath = strSavedRecPath,
                    baNewTimestamp = baNewTimestamp
                };
            }
        }

        #region SetReaderInfo()

        public class SetReaderInfoResponse
        {
            public LibraryServerResult SetReaderInfoResult { get; set; }
            public string strExistingXml { get; set; }
            public string strSavedXml { get; set; }
            public string strSavedRecPath { get; set; }

            // 注: 这里会用到 base64 string 输出 JSON
            public byte[] baNewTimestamp { get; set; }

            public ErrorCodeValue kernel_errorcode { get; set; }
        }

        public class SetReaderInfoRequest
        {
            public string strAction { get; set; }
            public string strRecPath { get; set; }
            public string strNewXml { get; set; }
            public string strOldXml { get; set; }

            // 注: 这里会用到 base64 string 输出 JSON
            public byte[] baOldTimestamp { get; set; }
        }

        #endregion

        // 移动读者记录
        [HttpPost]
        public MoveReaderInfoResponse MoveReaderInfo(
            [FromBody] MoveReaderInfoRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                string strTargetRecPath = request.strTargetRecPath;
                var result = service.MoveReaderInfo(
                    request.strSourceRecPath,
                    ref strTargetRecPath,
                    out byte[] target_timestamp);
                return new MoveReaderInfoResponse
                {
                    MoveReaderInfoResult = result,
                    strTargetRecPath = strTargetRecPath,
                    target_timestamp = target_timestamp,
                };
            }
        }

        [HttpGet]
        public MoveReaderInfoResponse MoveReaderInfo(
    string strSourceRecPath,
    string strTargetRecPath)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.MoveReaderInfo(
                    strSourceRecPath,
                    ref strTargetRecPath,
                    out byte[] target_timestamp);
                return new MoveReaderInfoResponse
                {
                    MoveReaderInfoResult = result,
                    strTargetRecPath = strTargetRecPath,
                    target_timestamp = target_timestamp,
                };
            }
        }

        #region MoveReaderInfo()

        public class MoveReaderInfoResponse
        {
            public LibraryServerResult MoveReaderInfoResult { get; set; }

            public string strTargetRecPath { get; set; }
            public byte[] target_timestamp { get; set; }
        }

        public class MoveReaderInfoRequest
        {
            public string strSourceRecPath { get; set; }
            public string strTargetRecPath { get; set; }
        }

        #endregion
    }

}
