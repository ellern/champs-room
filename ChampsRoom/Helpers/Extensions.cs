using ChampsRoom.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace ChampsRoom.Helpers
{
    public static class IdentityExtensions
    {
        public static string GetImageUrl(this IIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;

            if (claimsIdentity == null)
                return null;

            return claimsIdentity.FindFirstValue("ImageUrl");
        }
    }

    public static class IntExtensions
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
        public static string GetIconArrowClass(this int value)
        {
            if (value == 0)
                return "fa-arrow-right";

            if (value > 0)
                return "fa-arrow-up";

            if (value < 0)
                return "fa-arrow-down";

            return String.Empty;
        }
        public static string GetLabelClass(this int value)
        {
            if (value == 0)
                return "label-default";

            if (value > 0)
                return "label-success";

            if (value < 0)
                return "label-danger";

            return String.Empty;
        }    
    }

    public static class StringExtensions
    {
        public static string ToFriendlyUrl(this string s)
        {
            s = System.Text.RegularExpressions.Regex
                .Replace((s ?? "").ToLowerInvariant()
                .Replace("&", "-and-")
                .Replace("æ", "ae")
                .Replace("ø", "oe")
                .Replace("å", "aa"), @"[^a-z0-9]", "-");
            
            s = System.Text.RegularExpressions.Regex
                .Replace(s, @"-+", "-")
                .Trim()
                .Trim('-');

            return s;
        }
    }

    public static class MiscExtensions
    {
        public static List<RatingsViewModel> OrderByRanking(this List<RatingsViewModel> ratingViewModels)
        {
            return ratingViewModels
                    .OrderByDescending(q => q.Rate)
                    .ThenByDescending(q => q.Score)
                    .ThenByDescending(q => q.Won)
                    .ThenByDescending(q => q.Draw)
                    .ThenBy(q => q.Matches)
                    .ThenBy(q => q.User.UserName)
                    .ToList();
        }
    }
}