using System;
using System.Text;
using EnsureThat;

namespace Requester.Http
{
    public class BasicAuthorizationString
    {
        public string Value { get; private set; }

        public BasicAuthorizationString(string username, string password)
        {
            Ensure.That(username, "username").IsNotNullOrEmpty();
            Ensure.That(password, "password").IsNotNullOrEmpty();

            Value = GenerateBasicAuthorizationCredentials(username, password);
        }

        private string GenerateBasicAuthorizationCredentials(string username, string password)
        {
            var credentialsBytes = Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));

            return Convert.ToBase64String(credentialsBytes);
        }

        public static implicit operator string(BasicAuthorizationString item)
        {
            return item.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}