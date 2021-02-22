using GitProxyApp.Tunnels.Credentials;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitProxyApp.Tunnels
{
    public class SshTunnel : ITunnel
    {
        protected SshClient Client { get; }
        protected SshTunnel(SshClient client)
        {
            if (!client.IsConnected)
                throw new ArgumentException("Client is not connected.");

            Client = client;
        }
        public SshClient GetClient() => Client;
        public static SshTunnel Open(string hostname, SshCredentials credentials, int port = 22)
        {
            List<AuthenticationMethod> authenticationMethods = new List<AuthenticationMethod>();
            if (credentials.HasPassword)
                authenticationMethods.Add(new PasswordAuthenticationMethod(credentials.Username, credentials.Password));
            if (credentials.HasPrivateKeyFile)
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(credentials.PrivateKeyFile));

            ConnectionInfo connectionInfo = new ConnectionInfo(hostname, port, credentials.Username, authenticationMethods.ToArray());

            SshClient client = new SshClient(connectionInfo);
            client.Connect();

            return new SshTunnel(client);
        }
        public string Identifier => "ssh.net";

        public bool IsOpen => Client.IsConnected;

        public void Close()
        {
            if (Client.IsConnected)
                Client.Disconnect();

            Client.Dispose();
        }

        public GitSession OpenGitSession(GitSessionParameters parameters)
        {
            string cdCmd = $"cd \"{ parameters.WorkingDirectory }\"";
            SshCommand command = Client.CreateCommand(cdCmd);
            command.Execute();

            if (command.ExitStatus != 0)
                throw new InvalidOperationException("Failed to change to the specified Working Directory on the Remote host");

            command = Client.CreateCommand($"{cdCmd}; \"{ parameters.ExecutablePath }\" { parameters.QuotedArgumentString }");

            return new SshGitSession(command);
        }
    }
}
