using System;
using System.Runtime.Serialization;

namespace Open.Mega
{
    [DataContract]
    public class Node
    {
        public static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        [DataMember(Name = "h")]
        public string Id { get; private set; }
        [DataMember(Name = "p")]
        public string ParentId { get; set; }
        [DataMember(Name = "u")]
        public string Owner { get; private set; }
        [DataMember(Name = "t")]
        public NodeType Type { get; private set; }
        [DataMember(Name = "s")]
        public long Size { get; private set; }
        [DataMember(Name = "a")]
        public string SerializedAttributes { get; set; }
        [DataMember(Name = "k")]
        public string SerializedKey { get; set; }
        [DataMember(Name = "ts")]
        public long SerializedLastModificationDate { get; set; }
        [DataMember(Name = "fa")]
        public string FileAttributes { get; set; }

        public string Name { get; set; }
        public DateTime LastModificationDate { get; set; }
        public byte[] Iv { get; set; }
        public byte[] MetaMac { get; set; }
        public byte[] Key { get; set; }
    }

    [DataContract]
    public enum NodeType
    {
        File = 0,
        Directory = 1,
        Root = 2,
        Inbox = 3,
        Trash = 4,
    }
}
