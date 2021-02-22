using System;
using System.Collections.Generic;
using System.Text;

namespace GitProxyApp.Tunnels.Credentials
{
    public struct SshCredentials
    {
        public SshCredentials(string username, string password, string privateKeyFile)
        {
            Username = username;
            Password = password;
            PrivateKeyFile = privateKeyFile;
        }

        public string Username { get; }
        public string Password { get; }
        public string PrivateKeyFile { get; }

        public bool HasPassword => !string.IsNullOrWhiteSpace(Password);
        public bool HasUsername => !string.IsNullOrWhiteSpace(Username);
        public bool HasPrivateKeyFile => !string.IsNullOrWhiteSpace(PrivateKeyFile);
    }
}
