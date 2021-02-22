using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GitProxyApp.Tunnels
{
    public class SshGitSession : GitSession
    {
        protected bool IsFinished { get; set; }
        protected IAsyncResult AsyncHandler { get; }
        protected SshCommand Command { get; }
        public override bool IsRunning { get => IsFinished; }

        public override string StdOut => Command.Result;

        public override string StdError => Command.Error;

        public SshGitSession(SshCommand targetCommand)
            :base()
        {
            Command = targetCommand;
            AsyncHandler = targetCommand.BeginExecute();
            OutStream = targetCommand.OutputStream;
            ErrorStream = null;
            InStream = null;

            IsFinished = false;
        }

        protected override void FinishStream()
        {
            if (IsFinished)
                return;
            IsFinished = true;
            Command.EndExecute(AsyncHandler);
        }

        public override string ReadToEnd()
        {
            FinishStream();

            return Command.Result;
        }

        public override int GetExitCode() => Command.ExitStatus;
    }
}
