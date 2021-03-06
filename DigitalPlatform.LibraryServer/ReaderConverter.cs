using System;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.Text;
using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer
{
    // 操作类型
    // 可以提示创建页面的细节
    public enum OperType
    {
        None = 0,   // 既不是借书也不是还书
        Borrow = 1, // 借书
        Return = 2, // 还书
    }

    /// <summary>
    /// dp2library中调用C#脚本时, 用于转换读者信息xml->html的脚本类的基类
    /// </summary>
    public class ReaderConverter
    {
        public LibraryApplication App = null;
        public SessionInfo SessionInfo = null;

        public string[] BorrowedItemBarcodes = null;
        public string CurrentItemBarcode = "";  // 当前正在操作的条码号
        public OperType OperType = OperType.None;
        public string RecPath = ""; // 读者记录路径

        public string LibraryCode = ""; // 读者记录所从属的读者库的图书馆代码 2012/9/8

        public string Formats = ""; // 子格式。为逗号间隔的字符串列表 2013/12/4

        public static string LocalTime(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "G");
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "时间字符串 '" + strRfc1123Time + "' 格式错误，不是合法的RFC1123格式";
            }
        }

        public static string LocalDate(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";

                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123Time, "d"); // "yyyy-MM-dd"
            }
            catch (Exception /*ex*/)    // 2008/10/28
            {
                return "日期字符串 '" + strRfc1123Time + "' 格式错误，不是合法的RFC1123格式";
            }
        }

        // 获得流通参数
        public string GetParam(string strReaderType,
            string strBookType,
            string strParamName)
        {
            string strError = "";
            string strParamValue = "";
            MatchResult matchresult;
            int nRet = this.App.GetLoanParam(
                //null,
                this.LibraryCode,
                strReaderType, 
                strBookType,
                strParamName,
                out strParamValue,
                out matchresult,
                out strError);
            if (nRet == -1 || nRet == 0)
                return strError;

            // 2014/1/28
            // 这是读者特有的参数，要求必须返回值在 3 和以上
            if (string.IsNullOrEmpty(strBookType) == true && nRet < 3)
                return strError;
            if (string.IsNullOrEmpty(strBookType) == false && nRet < 4)
                return strError;

            return strParamValue;
        }

        // 实用函数：看看一个条码号是否为最近已经借阅过的册条码号
        public bool IsRecentBorrowedItem(string strBarcode)
        {
            if (BorrowedItemBarcodes == null)
                return false;

            for (int i = 0; i < BorrowedItemBarcodes.Length; i++)
            {
                if (strBarcode == this.BorrowedItemBarcodes[i])
                    return true;
            }

            return false;
        }

        public ReaderConverter()
        {

        }

        public virtual string Convert(string strXml)
        {

            return strXml;
        }
    }
}

