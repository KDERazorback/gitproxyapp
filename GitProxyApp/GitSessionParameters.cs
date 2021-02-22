using System;
using System.Collections.Generic;
using System.Text;

namespace GitProxyApp
{
    public struct GitSessionParameters
    {
        public string ExecutablePath { get; set; }
        public string WorkingDirectory { get; set; }
        public string[] Arguments { get; set; }
        public string QuotedArgumentString
        {
            get
            {
                StringBuilder str = new StringBuilder();
                foreach (string a in Arguments)
                {
                    string arg = a.Replace("\"", "\\\"");

                    if (str.Length > 0)
                        str.Append(' ');

                    str.Append('"');
                    str.Append(arg);
                    str.Append('"');
                }

                return str.ToString();
            }
        }
    }
}
