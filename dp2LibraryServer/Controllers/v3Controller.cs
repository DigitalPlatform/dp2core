using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.rms;

namespace dp2LibraryServer.Controllers
{
    [ApiController]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "v3")]
    //[Route("dp2library/[controller]/{instance}/[action]")]
    //[Route("dp2library/[controller]/[action]")]
    [Route("dp2library/v3/{instance}/[action]")]
    [Route("dp2library/v3/[action]")]
    public class v3Controller : ControllerBase
    {
        private readonly ILogger<v3Controller> _logger;

        public v3Controller(ILogger<v3Controller> logger)
        {
            _logger = logger;
        }

        [HttpPost]
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
        public LogoutResponse Logout()
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.Logout();
                return new LogoutResponse { LogoutResult = result };
            }
        }

#if REMOVED
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
#endif

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
                    baTimestamp = From(baTimestamp)
                };
            }
        }

        #region GetReaderInfo()

        public class GetReaderInfoResponse
        {
            public LibraryServerResult GetReaderInfoResult { get; set; }
            public string[] results { get; set; }
            public string strRecPath { get; set; }

            public int[] baTimestamp { get; set; }
        }

        // 采用 int [] 的 byte [] 转换器，兼容以前的 WCF Restful API 做法
        public class LegacyByteArrayConverter : JsonConverter<byte[]>
        {
            public override bool CanConvert(Type typeToConvert) =>
    typeof(byte[]).IsAssignableFrom(typeToConvert);

            public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }
                reader.Read();

                var elements = new List<byte>();

                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    elements.Add(JsonSerializer.Deserialize<byte>(ref reader, options));
                    reader.Read();
                }

                return elements.ToArray();
            }

            public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                foreach (var b in value)
                {
                    writer.WriteNumberValue((int)b);
                }
                writer.WriteEndArray();
            }
        }

        #region AutoByteArrayConverter (能自动适应 base64 string 和 int array 输入的 byte[] 输入输出转换器)

        // TODO: 处理并发请求会有问题
        public static bool _hasGetBytesFromBase64 = false;

        public class AutoByteArrayConverter : JsonConverter<byte[]>
        {
            public override bool CanConvert(Type typeToConvert) =>
    typeof(byte[]).IsAssignableFrom(typeToConvert);

            public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    _hasGetBytesFromBase64 = true;
                    return reader.GetBytesFromBase64();
                }

                // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }
                reader.Read();

                var elements = new List<byte>();

                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    elements.Add(JsonSerializer.Deserialize<byte>(ref reader, options));
                    reader.Read();
                }

                return elements.ToArray();
            }

            public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
            {
                if (_hasGetBytesFromBase64)
                {
                    writer.WriteBase64StringValue(value);
                    return;
                }

                writer.WriteStartArray();
                foreach (var b in value)
                {
                    writer.WriteNumberValue((int)b);
                }
                writer.WriteEndArray();
            }
        }

        #endregion

        public class GetReaderInfoRequest
        {
            public string strBarcode { get; set; }
            public string strResultTypeList { get; set; }
        }

        #endregion

        // 测试验证 byte []
        [HttpPost]
        public int[] TestByteArray([FromBody] TestByteArrayRequest request)
        {
            var result = From(request.Data);
            return From(result);
        }

        public class TestByteArrayRequest
        {
            // 注： Request 中 byte [] 要写为 int []
            public int[] Data { get; set; }
        }

        // 设置读者记录
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
    From(request.baOldTimestamp),
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
                    baNewTimestamp = From(baNewTimestamp)
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

            public int[] baNewTimestamp { get; set; }

            public ErrorCodeValue kernel_errorcode { get; set; }
        }

        public class SetReaderInfoRequest
        {
            public string strAction { get; set; }
            public string strRecPath { get; set; }
            public string strNewXml { get; set; }
            public string strOldXml { get; set; }

            public int[] baOldTimestamp { get; set; }
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

        [HttpPost]
        public DevolveReaderInfoResponse DevolveReaderInfo(
            [FromBody] DevolveReaderInfoRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.DevolveReaderInfo(
                    request.strSourceReaderBarcode,
                    request.strTargetReaderBarcode);
                return new DevolveReaderInfoResponse
                {
                    DevolveReaderInfoResult = result,
                };
            }
        }

        #region DevolveReaderInfo()

        public class DevolveReaderInfoResponse
        {
            public LibraryServerResult DevolveReaderInfoResult { get; set; }
        }

        public class DevolveReaderInfoRequest
        {
            public string strSourceReaderBarcode { get; set; }
            public string strTargetReaderBarcode { get; set; }
        }

        #endregion

        // 检索读者库
        [HttpPost]
        public SearchReaderResponse SearchReader(
    [FromBody] SearchReaderRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SearchReader(
                    request.strReaderDbNames,
    request.strQueryWord,
    request.nPerMax,
    request.strFrom,
    request.strMatchStyle,
    request.strLang,
    request.strResultSetName,
    request.strOutputStyle);
                return new SearchReaderResponse
                {
                    SearchReaderResult = result,
                };
            }
        }

        #region SearchReader()

        public class SearchReaderResponse
        {
            public LibraryServerResult SearchReaderResult { get; set; }
        }

        public class SearchReaderRequest
        {
            public string strReaderDbNames { get; set; }
            public string strQueryWord { get; set; }
            public int nPerMax { get; set; }
            public string strFrom { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public string strResultSetName { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public SetFriendsResponse SetFriends(
            [FromBody] SetFriendsRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SetFriends(
                    request.strAction,
request.strReaderBarcode,
request.strComment,
request.strStyle);
                return new SetFriendsResponse
                {
                    SetFriendsResult = result,
                };
            }
        }

        #region SetFriends()

        public class SetFriendsResponse
        {
            public LibraryServerResult SetFriendsResult { get; set; }
        }

        public class SetFriendsRequest
        {
            public string strAction { get; set; }
            public string strReaderBarcode { get; set; }
            public string strComment { get; set; }
            public string strStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public SearchOneDbResponse SearchOneDb(
            [FromBody] SearchOneDbRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.SearchOneDb(
                    request.strQueryWord,
    request.strDbName,
    request.strFrom,
    request.strMatchStyle,
    request.strLang,
    request.lMaxCount,
    request.strResultSetName,
    request.strOutputStyle);
                return new SearchOneDbResponse
                {
                    SearchOneDbResult = result,
                };
            }
        }

        #region SearchOneDb()

        public class SearchOneDbResponse
        {
            public LibraryServerResult SearchOneDbResult { get; set; }
        }

        public class SearchOneDbRequest
        {
            public string strQueryWord { get; set; }
            public string strDbName { get; set; }
            public string strFrom { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public long lMaxCount { get; set; }
            public string strResultSetName { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        // 通用检索
        [HttpPost]
        public SearchResponse Search(
            [FromBody] SearchRequest request)
        {
            using (var service = ServiceStore.GetService(HttpContext))
            {
                var result = service.Search(
                    request.strQueryXml,
    request.strResultSetName,
    request.strOutputStyle);
                return new SearchResponse
                {
                    SearchResult = result,
                };
            }
        }

        #region Search()

        public class SearchResponse
        {
            public LibraryServerResult SearchResult { get; set; }
        }

        public class SearchRequest
        {
            public string strQueryXml { get; set; }
            public string strResultSetName { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        // 获得检索结果
        [HttpPost]
        public GetSearchResultResponse GetSearchResult(
            [FromBody] GetSearchResultRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetSearchResult(
                request.strResultSetName,
request.lStart,
request.lCount,
request.strBrowseInfoStyle,
request.strLang,
out Record[] searchresults);
            return new GetSearchResultResponse
            {
                GetSearchResultResult = result,
                searchresults = searchresults,
            };
        }

        #region GetSearchResult()

        public class GetSearchResultResponse
        {
            public LibraryServerResult GetSearchResultResult { get; set; }
            public Record[] searchresults { get; set; }
        }

        public class GetSearchResultRequest
        {
            public string strResultSetName { get; set; }
            public long lStart { get; set; }
            public long lCount { get; set; }
            public string strBrowseInfoStyle { get; set; }
            public string strLang { get; set; }
        }

        #endregion

        [HttpPost]
        public GetRecordResponse GetRecord(
            [FromBody] GetRecordRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetRecord(
                request.strPath,
    out byte[] timestamp,
    out string strXml);
            return new GetRecordResponse
            {
                GetRecordResult = result,
                timestamp = timestamp,
                strXml = strXml,
            };
        }

        #region GetRecord()

        public class GetRecordResponse
        {
            public LibraryServerResult GetRecordResult { get; set; }
            public byte[] timestamp { get; set; }
            public string strXml { get; set; }
        }

        public class GetRecordRequest
        {
            public string strPath { get; set; }
        }

        #endregion

        [HttpPost]
        public GetBrowseRecordsResponse GetBrowseRecords(
            [FromBody] GetBrowseRecordsRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetBrowseRecords(
                request.paths,
    request.strBrowseInfoStyle,
    out Record[] searchresults);
            return new GetBrowseRecordsResponse
            {
                GetBrowseRecordsResult = result,
                searchresults = searchresults,
            };
        }

        #region GetBrowseRecords()

        public class GetBrowseRecordsResponse
        {
            public LibraryServerResult GetBrowseRecordsResult { get; set; }
            public Record[] searchresults { get; set; }
        }

        public class GetBrowseRecordsRequest
        {
            public string[] paths { get; set; }
            public string strBrowseInfoStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public ListBiblioDbFromsResponse ListBiblioDbFroms(
            [FromBody] ListBiblioDbFromsRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.ListBiblioDbFroms(
                 request.strDbType,
    request.strLang,
    out BiblioDbFromInfo[] infos);
            return new ListBiblioDbFromsResponse
            {
                ListBiblioDbFromsResult = result,
                infos = infos,
            };
        }

        #region ListBiblioDbFroms()

        public class ListBiblioDbFromsResponse
        {
            public LibraryServerResult ListBiblioDbFromsResult { get; set; }
            public BiblioDbFromInfo[] infos { get; set; }
        }

        public class ListBiblioDbFromsRequest
        {
            public string strDbType { get; set; }
            public string strLang { get; set; }
        }

        #endregion

        [HttpPost]
        public SearchBiblioResponse SearchBiblio(
            [FromBody] SearchBiblioRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SearchBiblio(
                 request.strBiblioDbNames,
    request.strQueryWord,
    request.nPerMax,
    request.strFromStyle,
    request.strMatchStyle,
    request.strLang,
    request.strResultSetName,
    request.strSearchStyle,
    request.strOutputStyle,
    request.strLocationFilter,
    out string strQueryXml);
            return new SearchBiblioResponse
            {
                SearchBiblioResult = result,
                strQueryXml = strQueryXml,
            };
        }

        #region SearchBiblio()

        public class SearchBiblioResponse
        {
            public LibraryServerResult SearchBiblioResult { get; set; }
            public string strQueryXml { get; set; }
        }

        public class SearchBiblioRequest
        {
            public string strBiblioDbNames { get; set; }
            public string strQueryWord { get; set; }
            public int nPerMax { get; set; }
            public string strFromStyle { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public string strResultSetName { get; set; }
            public string strSearchStyle { get; set; }
            public string strOutputStyle { get; set; }
            public string strLocationFilter { get; set; }
        }

        #endregion

        // 设置书目记录
        [HttpPost]
        public SetBiblioInfoResponse SetBiblioInfo(
            [FromBody] SetBiblioInfoRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SetBiblioInfo(
                  request.strAction,
    request.strBiblioRecPath,
    request.strBiblioType,
    request.strBiblio,
    request.baTimestamp,
    request.strComment,
    request.strStyle,
    out string strOutputBiblioRecPath,
    out byte[] baOutputTimestamp);
            return new SetBiblioInfoResponse
            {
                SetBiblioInfoResult = result,
                strOutputBiblioRecPath = strOutputBiblioRecPath,
                baOutputTimestamp = baOutputTimestamp,
            };
        }

        #region SetBiblioInfo()

        public class SetBiblioInfoResponse
        {
            public LibraryServerResult SetBiblioInfoResult { get; set; }
            public string strOutputBiblioRecPath { get; set; }
            public byte[] baOutputTimestamp { get; set; }
        }

        public class SetBiblioInfoRequest
        {
            public string strAction { get; set; }
            public string strBiblioRecPath { get; set; }
            public string strBiblioType { get; set; }
            public string strBiblio { get; set; }
            public byte[] baTimestamp { get; set; }
            public string strComment { get; set; }
            public string strStyle { get; set; }
        }

        #endregion

        // 复制书目记录
        [HttpPost]
        public CopyBiblioInfoResponse CopyBiblioInfo(
            [FromBody] CopyBiblioInfoRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.CopyBiblioInfo(
                  request.strAction,
    request.strBiblioRecPath,
    request.strBiblioType,
    request.strBiblio,
    request.baTimestamp,
    request.strNewBiblioRecPath,
    request.strNewBiblio,
    request.strMergeStyle,
    out string strOutputBiblio,
    out string strOutputBiblioRecPath,
    out byte[] baOutputTimestamp);
            return new CopyBiblioInfoResponse
            {
                CopyBiblioInfoResult = result,
                strOutputBiblio = strOutputBiblio,
                strOutputBiblioRecPath = strOutputBiblioRecPath,
                baOutputTimestamp = baOutputTimestamp,
            };
        }

        #region CopyBiblioInfo()

        public class CopyBiblioInfoResponse
        {
            public LibraryServerResult CopyBiblioInfoResult { get; set; }
            public string strOutputBiblio { get; set; }
            public string strOutputBiblioRecPath { get; set; }
            public byte[] baOutputTimestamp { get; set; }
        }

        public class CopyBiblioInfoRequest
        {
            public string strAction { get; set; }
            public string strBiblioRecPath { get; set; }
            public string strBiblioType { get; set; }
            public string strBiblio { get; set; }
            public byte[] baTimestamp { get; set; }
            public string strNewBiblioRecPath { get; set; }
            public string strNewBiblio { get; set; }
            public string strMergeStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public GetBiblioInfoResponse GetBiblioInfo(
            [FromBody] GetBiblioInfoRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetBiblioInfo(
                 request.strBiblioRecPath,
    request.strBiblioXml,
    request.strBiblioType,
    out string strBiblio);
            return new GetBiblioInfoResponse
            {
                GetBiblioInfoResult = result,
                strBiblio = strBiblio,
            };
        }

        #region GetBiblioInfo()

        public class GetBiblioInfoResponse
        {
            public LibraryServerResult GetBiblioInfoResult { get; set; }
            public string strBiblio { get; set; }
        }

        public class GetBiblioInfoRequest
        {
            public string strBiblioRecPath { get; set; }
            public string strBiblioXml { get; set; }
            public string strBiblioType { get; set; }
        }

        #endregion

        [HttpPost]
        public GetBiblioInfosResponse GetBiblioInfos(
            [FromBody] GetBiblioInfosRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetBiblioInfos(
                 request.strBiblioRecPath,
    request.strBiblioXml,
    request.formats,
    out string[] results,
    out byte[] baTimestamp);
            return new GetBiblioInfosResponse
            {
                GetBiblioInfosResult = result,
                results = results,
                baTimestamp = baTimestamp,
            };
        }

        #region GetBiblioInfos()

        public class GetBiblioInfosResponse
        {
            public LibraryServerResult GetBiblioInfosResult { get; set; }
            public string[] results { get; set; }
            public byte[] baTimestamp { get; set; }
        }

        public class GetBiblioInfosRequest
        {
            public string strBiblioRecPath { get; set; }
            public string strBiblioXml { get; set; }
            public string[] formats { get; set; }
        }

        #endregion

        [HttpPost]
        public SearchItemResponse SearchItem(
            [FromBody] SearchItemRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SearchItem(
                 request.strItemDbName,
    request.strQueryWord,
    request.nPerMax,
    request.strFrom,
    request.strMatchStyle,
    request.strLang,
    request.strResultSetName,
    request.strSearchStyle,
    request.strOutputStyle);
            return new SearchItemResponse
            {
                SearchItemResult = result,
            };
        }

        #region SearchItem()

        public class SearchItemResponse
        {
            public LibraryServerResult SearchItemResult { get; set; }
        }

        public class SearchItemRequest
        {
            public string strItemDbName { get; set; }
            public string strQueryWord { get; set; }
            public int nPerMax { get; set; }
            public string strFrom { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public string strResultSetName { get; set; }
            public string strSearchStyle { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public GetItemInfoResponse GetItemInfo(
            [FromBody] GetItemInfoRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetItemInfo(
                 request.strItemDbType,
    request.strBarcode,
    request.strItemXml,
    request.strResultType,
    out string strResult,
    out string strItemRecPath,
    out byte[] item_timestamp,
    request.strBiblioType,
    out string strBiblio,
    out string strBiblioRecPath);
            return new GetItemInfoResponse
            {
                GetItemInfoResult = result,
                strResult = strResult,
                strItemRecPath = strItemRecPath,
                item_timestamp = item_timestamp,
                strBiblio = strBiblio,
                strBiblioRecPath = strBiblioRecPath
            };
        }

        #region GetItemInfo()

        public class GetItemInfoResponse
        {
            public LibraryServerResult GetItemInfoResult { get; set; }

            public string strResult { get; set; }
            public string strItemRecPath { get; set; }
            public byte[] item_timestamp { get; set; }

            public string strBiblio { get; set; }
            public string strBiblioRecPath { get; set; }
        }

        public class GetItemInfoRequest
        {
            public string strItemDbType { get; set; }
            public string strBarcode { get; set; }
            public string strItemXml { get; set; }
            public string strResultType { get; set; }
            public string strBiblioType { get; set; }
        }

        #endregion

        [HttpPost]
        public GetBiblioSummaryResponse GetBiblioSummary(
            [FromBody] GetBiblioSummaryRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetBiblioSummary(
                 request.strItemBarcode,
            request.strConfirmItemRecPath,
            request.strBiblioRecPathExclude,
            out string strBiblioRecPath,
            out string strSummary);
            return new GetBiblioSummaryResponse
            {
                GetBiblioSummaryResult = result,
                strBiblioRecPath = strBiblioRecPath,
                strSummary = strSummary,
            };
        }

        #region GetBiblioSummary()

        public class GetBiblioSummaryResponse
        {
            public LibraryServerResult GetBiblioSummaryResult { get; set; }
            public string strBiblioRecPath { get; set; }
            public string strSummary { get; set; }
        }

        public class GetBiblioSummaryRequest
        {
            public string strItemBarcode { get; set; }
            public string strConfirmItemRecPath { get; set; }
            public string strBiblioRecPathExclude { get; set; }
        }

        #endregion

        [HttpPost]
        public BorrowResponse Borrow(
            [FromBody] BorrowRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.Borrow(
                 request.bRenew,
            request.strReaderBarcode,
            request.strItemBarcode,
            request.strConfirmItemRecPath,
            request.bForce,
            request.saBorrowedItemBarcode,
            request.strStyle,
            request.strItemFormatList,
            out string[] item_records,
            request.strReaderFormatList,
            out string[] reader_records,
            request.strBiblioFormatList,
            out string[] biblio_records,
            out BorrowInfo borrow_info,
            out string[] aDupPath,
            out string strOutputReaderBarcode);
            return new BorrowResponse
            {
                BorrowResult = result,
                item_records = item_records,
                reader_records = reader_records,
                biblio_records = biblio_records,
                borrow_info = borrow_info,
                aDupPath = aDupPath,
                strOutputReaderBarcode = strOutputReaderBarcode,
            };
        }

        #region Borrow()

        public class BorrowResponse
        {
            public LibraryServerResult BorrowResult { get; set; }

            public string[] item_records { get; set; }
            public string[] reader_records { get; set; }
            public string[] biblio_records { get; set; }
            public BorrowInfo borrow_info { get; set; }
            public string[] aDupPath { get; set; }
            public string strOutputReaderBarcode { get; set; }
        }

        public class BorrowRequest
        {
            public bool bRenew { get; set; }
            public string strReaderBarcode { get; set; }
            public string strItemBarcode { get; set; }
            public string strConfirmItemRecPath { get; set; }
            public bool bForce { get; set; }
            public string[] saBorrowedItemBarcode { get; set; }
            public string strStyle { get; set; }
            public string strItemFormatList { get; set; }
            public string strReaderFormatList { get; set; }
            public string strBiblioFormatList { get; set; }
        }

        #endregion

        [HttpPost]
        public ReturnResponse Return(
            [FromBody] ReturnRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.Return(
                 request.strAction,
            request.strReaderBarcode,
            request.strItemBarcode,
            request.strComfirmItemRecPath,
            request.bForce,
            request.strStyle,
            request.strItemFormatList,
            out string[] item_records,
            request.strReaderFormatList,
            out string[] reader_records,
            request.strBiblioFormatList,
            out string[] biblio_records,
            out string[] aDupPath,
            out string strOutputReaderBarcode,
            out ReturnInfo return_info);
            return new ReturnResponse
            {
                ReturnResult = result,
                item_records = item_records,
                reader_records = reader_records,
                biblio_records = biblio_records,
                aDupPath = aDupPath,
                return_info = return_info,
                strOutputReaderBarcode = strOutputReaderBarcode,
            };
        }

        #region Return()

        public class ReturnResponse
        {
            public LibraryServerResult ReturnResult { get; set; }

            public string[] item_records { get; set; }
            public string[] reader_records { get; set; }
            public string[] biblio_records { get; set; }
            public string[] aDupPath { get; set; }
            public string strOutputReaderBarcode { get; set; }
            public ReturnInfo return_info { get; set; }
        }

        public class ReturnRequest
        {
            public string strAction { get; set; }
            public string strReaderBarcode { get; set; }
            public string strItemBarcode { get; set; }
            public string strComfirmItemRecPath { get; set; }
            public bool bForce { get; set; }
            public string strStyle { get; set; }
            public string strItemFormatList { get; set; }
            public string strReaderFormatList { get; set; }
            public string strBiblioFormatList { get; set; }
        }

        #endregion

        [HttpPost]
        public ReservationResponse Reservation(
            [FromBody] ReservationRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.Reservation(
                 request.strFunction,
            request.strReaderBarcode,
            request.strItemBarcodeList);
            return new ReservationResponse
            {
                ReservationResult = result,
            };
        }

        #region Reservation()

        public class ReservationResponse
        {
            public LibraryServerResult ReservationResult { get; set; }
        }

        public class ReservationRequest
        {
            public string strFunction { get; set; }
            public string strReaderBarcode { get; set; }
            public string strItemBarcodeList { get; set; }
        }

        #endregion

        [HttpPost]
        public AmerceResponse Amerce(
            [FromBody] AmerceRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.Amerce(
                 request.strFunction,
            request.strReaderBarcode,
            request.amerce_items,
            out AmerceItem[] failed_items,
            out string strReaderXml);
            return new AmerceResponse
            {
                AmerceResult = result,
                failed_items = failed_items,
                strReaderXml = strReaderXml,
            };
        }

        #region Amerce()

        public class AmerceResponse
        {
            public LibraryServerResult AmerceResult { get; set; }

            public AmerceItem[] failed_items { get; set; }
            public string strReaderXml { get; set; }
        }

        public class AmerceRequest
        {
            public string strFunction { get; set; }
            public string strReaderBarcode { get; set; }
            public AmerceItem[] amerce_items { get; set; }
        }

        #endregion

        [HttpPost]
        public GetIssuesResponse GetIssues(
            [FromBody] GetIssuesRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetIssues(
                 request.strBiblioRecPath,
            request.lStart,
            request.lCount,
            request.strStyle,
            request.strLang,
            out EntityInfo[] issueinfos);
            return new GetIssuesResponse
            {
                GetIssuesResult = result,
                issueinfos = issueinfos,
            };
        }

        #region GetIssues()

        public class GetIssuesResponse
        {
            public LibraryServerResult GetIssuesResult { get; set; }
            public EntityInfo[] issueinfos { get; set; }
        }

        public class GetIssuesRequest
        {
            public string strBiblioRecPath { get; set; }
            public long lStart { get; set; }
            public long lCount { get; set; }
            public string strStyle { get; set; }
            public string strLang { get; set; }
        }

        #endregion

        [HttpPost]
        public SetIssuesResponse SetIssues(
            [FromBody] SetIssuesRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SetIssues(
                 request. strBiblioRecPath,
                request.issueinfos,
                out EntityInfo[] errorinfos);
            return new SetIssuesResponse
            {
                SetIssuesResult = result,
                errorinfos = errorinfos,
            };
        }

        #region SetIssues()

        public class SetIssuesResponse
        {
            public LibraryServerResult SetIssuesResult { get; set; }
            public EntityInfo[] errorinfos { get; set; }
        }

        public class SetIssuesRequest
        {
            public string strBiblioRecPath { get; set; }
            public EntityInfo[] issueinfos { get; set; }
        }

        #endregion

        [HttpPost]
        public SearchIssueResponse SearchIssue(
            [FromBody] SearchIssueRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SearchIssue(
                request. strIssueDbName,
            request.strQueryWord,
            request.nPerMax,
            request.strFrom,
            request.strMatchStyle,
            request.strLang,
            request.strResultSetName,
            request.strSearchStyle,
            request.strOutputStyle);
            return new SearchIssueResponse
            {
                SearchIssueResult = result,
            };
        }

        #region SearchIssue()

        public class SearchIssueResponse
        {
            public LibraryServerResult SearchIssueResult { get; set; }
        }

        public class SearchIssueRequest
        {
            public string strIssueDbName { get; set; }
            public string strQueryWord { get; set; }
            public int nPerMax { get; set; }
            public string strFrom { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public string strResultSetName { get; set; }
            public string strSearchStyle { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public GetEntitiesResponse GetEntities(
           [FromBody] GetEntitiesRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetEntities(
                request. strBiblioRecPath,
           request.lStart,
           request.lCount,
           request.strStyle,
           request.strLang,
           out EntityInfo[] entityinfos);
            return new GetEntitiesResponse
            {
                GetEntitiesResult = result,
                entityinfos = entityinfos,
            };
        }

        #region GetEntities()

        public class GetEntitiesResponse
        {
            public LibraryServerResult GetEntitiesResult { get; set; }
            public EntityInfo[] entityinfos { get; set; }
        }

        public class GetEntitiesRequest
        {
            public string strBiblioRecPath { get; set; }
            public long lStart { get; set; }
            public long lCount { get; set; }
            public string strStyle { get; set; }
            public string strLang { get; set; }
        }

        #endregion

        [HttpPost]
        public SetEntitiesResponse SetEntities(
    [FromBody] SetEntitiesRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SetEntities(
                request.strBiblioRecPath,
                request.entityinfos,
                out EntityInfo[] errorinfos);
            return new SetEntitiesResponse
            {
                SetEntitiesResult = result,
                errorinfos = errorinfos,
            };
        }

        #region SetEntities()

        public class SetEntitiesResponse
        {
            public LibraryServerResult SetEntitiesResult { get; set; }
            public EntityInfo[] errorinfos { get; set; }
        }

        public class SetEntitiesRequest
        {
            public string strBiblioRecPath { get; set; }
            public EntityInfo[] entityinfos { get; set; }
        }

        #endregion

        [HttpPost]
        public GetOrdersResponse GetOrders(
    [FromBody] GetOrdersRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.GetOrders(
                 request.strBiblioRecPath,
            request.lStart,
            request.lCount,
            request.strStyle,
            request.strLang,
            out EntityInfo[] orderinfos);
            return new GetOrdersResponse
            {
                GetOrdersResult = result,
                orderinfos = orderinfos,
            };
        }

        #region GetOrders()

        public class GetOrdersResponse
        {
            public LibraryServerResult GetOrdersResult { get; set; }
            public EntityInfo[] orderinfos { get; set; }
        }

        public class GetOrdersRequest
        {
            public string strBiblioRecPath { get; set; }
            public long lStart { get; set; }
            public long lCount { get; set; }
            public string strStyle { get; set; }
            public string strLang { get; set; }
        }

        #endregion

        [HttpPost]
        public SetOrdersResponse SetOrders(
    [FromBody] SetOrdersRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SetOrders(
                request.strBiblioRecPath,
                request.orderinfos,
                out EntityInfo[] errorinfos);
            return new SetOrdersResponse
            {
                SetOrdersResult = result,
                errorinfos = errorinfos,
            };
        }

        #region SetOrders()

        public class SetOrdersResponse
        {
            public LibraryServerResult SetOrdersResult { get; set; }
            public EntityInfo[] errorinfos { get; set; }
        }

        public class SetOrdersRequest
        {
            public string strBiblioRecPath { get; set; }
            public EntityInfo[] orderinfos { get; set; }
        }

        #endregion

        [HttpPost]
        public SearchOrderResponse SearchOrder(
    [FromBody] SearchOrderRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SearchOrder(
                request.strOrderDbName,
            request.strQueryWord,
            request.nPerMax,
            request.strFrom,
            request.strMatchStyle,
            request.strLang,
            request.strResultSetName,
            request.strSearchStyle,
            request.strOutputStyle);
            return new SearchOrderResponse
            {
                SearchOrderResult = result,
            };
        }

        #region SearchOrder()

        public class SearchOrderResponse
        {
            public LibraryServerResult SearchOrderResult { get; set; }
        }

        public class SearchOrderRequest
        {
            public string strOrderDbName { get; set; }
            public string strQueryWord { get; set; }
            public int nPerMax { get; set; }
            public string strFrom { get; set; }
            public string strMatchStyle { get; set; }
            public string strLang { get; set; }
            public string strResultSetName { get; set; }
            public string strSearchStyle { get; set; }
            public string strOutputStyle { get; set; }
        }

        #endregion

        [HttpPost]
        public SetClockResponse SetClock([FromBody] SetClockRequest request)
        {
            using var service = ServiceStore.GetService(HttpContext);
            var result = service.SetClock(
                request. strTime);
            return new SetClockResponse
            {
                SetClockResult = result,
            };
        }

        #region SetClock()

        public class SetClockResponse
        {
            public LibraryServerResult SetClockResult { get; set; }
        }

        public class SetClockRequest
        {
            public string strTime { get; set; }
        }

        #endregion

        [HttpPost]
        public GetClockResponse GetClock()
        {
            using var service = ServiceStore.GetService(HttpContext, false);
            var result = service.GetClock(out string strTime);
            return new GetClockResponse
            {
                GetClockResult = result,
                strTime = strTime,
            };
        }

        public class GetClockResponse
        {
            public LibraryServerResult GetClockResult { get; set; }
            public string strTime { get; set; }
        }


        #region 实用函数

        static byte[] From(int[] array)
        {
            if (array == null)
                return null;
            return array.MyCast<byte, int>().ToArray();
        }

        /* // 会抛出异常
        static int[] From(byte[] array)
        {
            if (array == null)
                return null;
            return array.Cast<int>().ToArray();
        }
        */

        static int[] From(byte[] array)
        {
            if (array == null)
                return null;
            return array.MyCast<int, byte>().ToArray();
        }

        #endregion
    }

    // 能正确运行的 Cast() 实用函数
    // https://stackoverflow.com/questions/61417462/unable-to-cast-object-of-type-system-int32-to-type-system-int64
    public static class MyConvertExtension
    {
        public static IEnumerable<TDest> MyCast<TDest, TSource>(this IEnumerable<TSource> col)
        {
            foreach (var s in col)
                yield return (TDest)Convert.ChangeType(s, typeof(TDest));
        }
    }
}
