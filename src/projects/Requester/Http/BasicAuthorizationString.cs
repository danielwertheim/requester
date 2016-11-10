using System;
using System.Text;
using EnsureThat;

namespace Requester.Http
{
    public class BasicAuthorizationString : IEquatable<string>, IEquatable<BasicAuthorizationString>
    {
        public string Value { get; }

        public BasicAuthorizationString(string username, string password)
        {
            Ensure.That(username, "username").IsNotNullOrEmpty();
            Ensure.That(password, "password").IsNotNullOrEmpty();

            Value = GenerateBasicAuthorizationCredentials(username, password);
        }

        private static string GenerateBasicAuthorizationCredentials(string username, string password)
        {
            var credentialsBytes = Encoding.UTF8.GetBytes($"{username}:{password}");

            return Convert.ToBase64String(credentialsBytes);
        }

        public static implicit operator string(BasicAuthorizationString item) => item.Value;

        public override bool Equals(object obj)
        {
            if (obj is string)
                return Equals(obj as string);

            if (obj is BasicAuthorizationString)
                return Equals(obj as BasicAuthorizationString);

            return false;
        }

        public bool Equals(BasicAuthorizationString other)
        {
            return Equals(other?.Value);
        }

        public bool Equals(string other)
        {
            return string.Equals(Value, other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}