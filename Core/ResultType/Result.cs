using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ResultType
{
    public class Result<T> : IResult<T>
    {
        public bool IsSuccess { get; set; }
        public ResultTypeEnum ResultType { get; set; }
        public string Html { get; set; }
        public string Message { get; set; }
        public string SecondMessage { get; set; } = "";
        public List<LangParams> LangMessages { get; set; }
        public bool IsLastPackage { get; set; }
        public T Data { get; set; }
        public int ResultCount { get; set; }
        public int PageCount { get; set; }

        public Result()
        {
            Data = default(T);
        }

        public Result(T Data)
            : this(true, ResultTypeEnum.None, string.Empty, Data, false, string.Empty, String.Empty)
        {
        }
        public Result(string Html, T Data)
            : this(true, ResultTypeEnum.None, Html, Data, false, string.Empty, String.Empty)
        {
        }
        public Result(T Data, bool IsLastPackage)
            : this(true, ResultTypeEnum.None, string.Empty, Data, IsLastPackage, string.Empty, String.Empty)
        {
        }
        public Result(T Data, string Message, params string[] parameters)
           : this(true, ResultTypeEnum.None, string.Empty, Data, false, Message, parameters)
        {
        }
        public Result(string Html, T Data, string Message, params string[] parameters)
           : this(true, ResultTypeEnum.None, Html, Data, false, Message, parameters)
        {
        }
        public Result(ResultTypeEnum ResultTypeEnum, string Html, T Data, string Message, params string[] parameters)
           : this(true, ResultTypeEnum, Html, Data, false, Message, parameters)
        {
        }
        public Result(ResultTypeEnum ResultTypeEnum, T Data, string Message, params string[] parameters)
          : this(true, ResultTypeEnum, string.Empty, Data, false, Message, parameters)
        {
        }
        public Result(T Data, bool IsLastPackage, string Message, params string[] parameters)
           : this(true, ResultTypeEnum.None, string.Empty, Data, IsLastPackage, Message, parameters)
        {
        }

        public Result(bool IsSuccess, string Message, params string[] parameters)
          : this(IsSuccess, ResultTypeEnum.None, string.Empty, default(T), false, Message, parameters)
        {
        }
        public Result(bool IsSuccess, ResultTypeEnum ResultType, string Message, params string[] parameters)
            : this(IsSuccess, ResultType, string.Empty, default(T), false, Message, parameters)
        {
        }

        public Result(bool IsSuccess, ResultTypeEnum ResultType, T Data, string Message, params string[] parameters)
            : this(IsSuccess, ResultType, string.Empty, Data, false, Message, parameters)
        {
        }

        public Result(bool IsSuccess, ResultTypeEnum ResultType, string Html, T Data, bool IsLastPackage, string Message, params string[] parameters)
        {
            this.IsSuccess = IsSuccess;
            this.ResultType = ResultType;
            this.Message = string.Format(Message, parameters);
            this.Html = Html;
            this.Data = Data;
            this.IsLastPackage = IsLastPackage;
        }

        public Result(bool IsSuccess, ResultTypeEnum ResultType, T Data, bool IsLastPackage, int PageCount,
            int ResultCount, string Html,string Message, params string[] parameters)
        {
            this.IsSuccess = IsSuccess;
            this.ResultType = ResultType;
            this.Data = Data;
            this.IsLastPackage = IsLastPackage;
            this.PageCount = PageCount;
            this.ResultCount = ResultCount;
            this.Html = Html;
            this.Message = string.Format(Message, parameters);
        }

        public void Import(IResult result)
        {
            this.IsSuccess = result.IsSuccess;
            this.ResultType = result.ResultType;
            this.Message = result.Message;
        }
    }

    public enum ResultTypeEnum
    {
        None = 0,
        Information = 1,
        Success = 2,
        Warning = 3,
        Error = 4
    };

    public class LangParams
    {
        public string Key { get; set; } // == "tr" || "en" || "de" || "ru" .......
        public string Message { get; set; }
    }
}


