using System;
using System.Linq;

namespace Requester.Validation
{
    public static class AssertionExceptionFactory
    {
        /// <summary>
        /// Extension point allowing you to hook in specific test framework
        /// exception instead of <see cref="RequesterAssertionException"/>.
        /// </summary>
        public static Func<string, Exception> ExceptionFn { private get; set; }

        static AssertionExceptionFactory()
        {
            ExceptionFn = m => new RequesterAssertionException(m);
        }

        public static Exception Create(string format, params object[] args)
        {
            var message = args == null || !args.Any()
                ? format
                : string.Format(format, args);

            return ExceptionFn(message);
        }

        public static Exception Create(HttpResponse response, string format, params object[] args)
        {
            var message = args == null || !args.Any()
                ? format
                : string.Format(format, args);

            return ExceptionFn(string.Format("{0}{1}{2}", message, Environment.NewLine, response.ToStringDebugVersion()));
        }
    }
}