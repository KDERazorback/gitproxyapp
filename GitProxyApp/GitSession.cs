using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GitProxyApp
{
    public abstract class GitSession : IDisposable
    {
        public abstract bool IsRunning { get; }
        public bool IsDisposed { get; protected set; }
        protected Stream InStream { get; set; }
        protected Stream OutStream { get; set; }
        protected Stream ErrorStream { get; set; }
        public abstract string ReadToEnd();
        public abstract int GetExitCode();
        protected GitSession() { }
        protected GitSession(Stream inStream, Stream outStream, Stream errorStream)
        {
            InStream = inStream;
            OutStream = outStream;
            ErrorStream = errorStream;
        }

        public Stream GetOutputStream() => OutStream;
        public Stream GetInputStream() => InStream;
        public Stream GetErrorStream() => ErrorStream;
        public abstract string StdOut { get; }
        public abstract string StdError { get; }
        public virtual bool HasFailed => GetExitCode() > 0;

        protected virtual void FinishStream() { }

        public void Dispose()
        {
            if (!IsDisposed)
                FinishStream();

            IsDisposed = true;

            InStream?.Dispose();
            OutStream?.Dispose();
            ErrorStream?.Dispose();
        }
    }
}
