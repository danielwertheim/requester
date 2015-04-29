using System;
using System.Linq;

namespace Requester.Extensions
{
    public static class UriExtensions
    {
        public static string GetAbsoluteAddressExceptUserInfo(this Uri uri)
        {
            return uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.UserInfo, UriFormat.UriEscaped);
        }

        public static BasicAuthorizationString GetBasicAuthString(this Uri uri)
        {
            if (string.IsNullOrWhiteSpace(uri.UserInfo))
                return null;

            var parts = uri.GetUserInfoParts();

            return new BasicAuthorizationString(parts[0], parts[1]);
        }

        public static string[] GetUserInfoParts(this Uri uri)
        {
            return uri.UserInfo
                .Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.UnescapeDataString)
                .ToArray();
        }
    }
}