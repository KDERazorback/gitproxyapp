using System;
using System.Collections.Generic;
using System.Text;

namespace GitProxyApp.Tunnels
{
    public interface ITunnel
    {
        public string Identifier { get; }
        public bool IsOpen { get; }
        public GitSession OpenGitSession(GitSessionParameters parameters);
        public void Close();
    }
}
