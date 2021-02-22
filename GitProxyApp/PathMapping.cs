using System.Runtime.Serialization;

namespace GitProxyApp
{
    [DataContract]
    public class PathMapping
    {
        public PathMapping(string localPath, string remotePath)
        {
            LocalPath = localPath;
            RemotePath = remotePath;
        }

        [DataMember]
        public string LocalPath { get; }
        [DataMember]
        public string RemotePath { get; }
    }
}