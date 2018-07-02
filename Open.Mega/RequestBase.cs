using System.Runtime.Serialization;

namespace Open.Mega
{
    [DataContract]
    public class RequestBase
    {
        public RequestBase(string action)
        {
            Action = action;
        }
        [DataMember(Name = "a")]
        public string Action { get; private set; }
    }

    [DataContract]
    public class LoginRequest : RequestBase
    {
        public LoginRequest(string userHandle, string passwordHash)
            : base("us")
        {
            this.UserHandle = userHandle;
            this.PasswordHash = passwordHash;
        }

        [DataMember(Name = "user")]
        public string UserHandle { get; private set; }

        [DataMember(Name = "uh")]
        public string PasswordHash { get; private set; }
    }

    [DataContract]
    public class LoginResponse
    {
        [DataMember(Name = "csid")]
        public string SessionId { get; private set; }

        [DataMember(Name = "tsid")]
        public string TemporarySessionId { get; private set; }

        [DataMember(Name = "privk")]
        public string PrivateKey { get; private set; }

        [DataMember(Name = "k")]
        public string MasterKey { get; private set; }
    }
    #region Nodes

    [DataContract]
    public class GetNodesRequest : RequestBase
    {
        public GetNodesRequest()
            : base("f")
        {
            this.c = 1;
        }

        [DataMember(Name = "c")]
        public int c { get; private set; }
    }

    [DataContract]
    public class GetNodesResponse
    {
        //public Node[] Nodes { get; private set; }

        [DataMember(Name = "f")]
        public Node[] Nodes { get; private set; }

        [DataMember(Name = "ok")]
        public SharedKey[] SharedKeys { get; private set; }
    }

    [DataContract]
    public class SharedKey
    {
        [DataMember(Name = "h")]
        public string Id { get; private set; }

        [DataMember(Name = "k")]
        public string Key { get; private set; }
    }

    #region Attributes

    [DataContract]
    public class Attributes
    {
        public Attributes(string name)
        {
            this.Name = name;
        }

        [DataMember(Name = "n")]
        public string Name { get; set; }
    }

    #endregion

    #region Create node

    [DataContract]
    public class CreateNodeRequest : RequestBase
    {
        private CreateNodeRequest(Node parentNode, NodeType type, string attributes, string key, string completionHandle)
            : base("p")
        {
            this.ParentId = parentNode.Id;
            this.Nodes = new[]
                {
                    new CreateNodeRequestData
                        {
                            Attributes = attributes,
                            Key = key,
                            Type = type,
                            CompletionHandle = completionHandle
                        }
                };
        }

        public static CreateNodeRequest CreateFileNodeRequest(Node parentNode, string attributes, string key, string completionHandle)
        {
            return new CreateNodeRequest(parentNode, NodeType.File, attributes, key, completionHandle);
        }

        public static CreateNodeRequest CreateFolderNodeRequest(Node parentNode, string attributes, string key)
        {
            return new CreateNodeRequest(parentNode, NodeType.Directory, attributes, key, "xxxxxxxx");
        }

        [DataMember(Name = "t")]
        public string ParentId { get; private set; }

        [DataMember(Name = "n")]
        public CreateNodeRequestData[] Nodes { get; private set; }
    }

    [DataContract]
    public class CreateNodeRequestData
    {
        [DataMember(Name = "h")]
        public string CompletionHandle { get; set; }

        [DataMember(Name = "t")]
        public NodeType Type { get; set; }

        [DataMember(Name = "a")]
        public string Attributes { get; set; }

        [DataMember(Name = "k")]
        public string Key { get; set; }
    }

    #endregion

    #region Delete

    [DataContract]
    public class DeleteRequest : RequestBase
    {
        public DeleteRequest(Node node)
            : base("d")
        {
            this.Node = node.Id;
        }

        [DataMember(Name = "n")]
        public string Node { get; private set; }
    }

    #endregion

    #region UploadRequest

    [DataContract]
    internal class UploadUrlRequest : RequestBase
    {
        public UploadUrlRequest(long fileSize)
            : base("u")
        {
            this.Size = fileSize;
        }

        [DataMember(Name = "s")]
        public long Size { get; private set; }
    }

    [DataContract]
    internal class UploadUrlResponse
    {
        [DataMember(Name = "p")]
        public string Url { get; private set; }
    }

    #endregion

    #region DownloadRequest

    [DataContract]
    public class DownloadUrlRequest : RequestBase
    {
        public DownloadUrlRequest(Node node)
            : base("g")
        {
            this.Id = node.Id;
        }

        [DataMember(Name = "g")]
        public int g { get { return 1; } set { } }

        [DataMember(Name = "n")]
        public string Id { get; private set; }
    }

    [DataContract]
    public class DownloadUrlResponse
    {
        [DataMember(Name = "g")]
        public string Url { get; private set; }

        [DataMember(Name = "s")]
        public long Size { get; private set; }

        [DataMember(Name = "at")]
        private string SerializedAttributes { get; set; }
    }

    #endregion

    #region Move

    [DataContract]
    public class MoveRequest : RequestBase
    {
        public MoveRequest(Node node, Node destinationParentNode)
            : base("m")
        {
            this.Id = node.Id;
            this.DestinationParentId = destinationParentNode.Id;
        }

        [DataMember(Name = "n")]
        public string Id { get; private set; }

        [DataMember(Name = "t")]
        public string DestinationParentId { get; private set; }
    }

    #endregion

    #region SetAttributes

    [DataContract]
    public class SetAttributesRequest : RequestBase
    {
        public SetAttributesRequest(Node node, string serializedAttributes, string key)
            : base("a")
        {
            this.Id = node.Id;
            this.SerializedAttributes = serializedAttributes;
            this.Key = key;
        }

        [DataMember(Name = "n")]
        public string Id { get; private set; }

        [DataMember(Name = "at")]
        public string SerializedAttributes { get; private set; }

        [DataMember(Name = "k")]
        public string Key { get; private set; }
    }

    #endregion

    #endregion

}
