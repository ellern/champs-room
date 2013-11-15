using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ChampsRoom.Helpers
{
    public static class Extentions
    {
        public static string DisplayChange(this int value)
        {
            if (value == 0)
                return "";

            if (value > 0)
                return "+" + value;

            if (value < 0)
                return value.ToString();

            return "";
        }
        public static string ValueIconClass(this int value)
        {
            if (value == 0)
                return "fa-arrow-right";

            if (value > 0)
                return "fa-arrow-up";

            if (value < 0)
                return "fa-arrow-down";

            return String.Empty;
        }

        public static string ValueLabelClass(this int value)
        {
            if (value == 0)
                return "label-default";

            if (value > 0)
                return "label-success";

            if (value < 0)
                return "label-danger";

            return String.Empty;
        }

        

        public static string ToFriendlyUrl(this string s)
        {
            return UrlExtentions.ToFriendlyUrl(null, s);
        }

        public static string AppendTrailingSlash(this string s)
        {
            if (s.EndsWith("/"))
                return s;

            return s + "/";
        }

        public static string RemoveTrailingSlash(this string s)
        {
            return s.TrimEnd('/');
        }
    }


    public static class UrlExtentions
    {
        public static string ToFriendlyUrl(this UrlHelper helper, string urlToEncode)
        {
            urlToEncode = System.Text.RegularExpressions.Regex.Replace((urlToEncode ?? "").ToLower().Replace("&", "-and-").Replace("æ", "ae").Replace("ø", "oe").Replace("å", "aa"), @"[^a-z0-9]", "-");
            urlToEncode = System.Text.RegularExpressions.Regex.Replace(urlToEncode, @"-+", "-").Trim().Trim('-');

            return urlToEncode;
        }
    }
}