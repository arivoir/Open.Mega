namespace Open.Mega;

public class RequestBase
{
    public RequestBase(string action)
    {
        Action = action;
    }
    [JsonPropertyName("a")]
    public string Action { get; private set; }
}

public class LoginRequest : RequestBase
{
    public LoginRequest(string userHandle, string passwordHash)
        : base("us")
    {
        UserHandle = userHandle;
        PasswordHash = passwordHash;
    }

    [JsonPropertyName("user")]
    public string UserHandle { get; private set; }

    [JsonPropertyName("uh")]
    public string PasswordHash { get; private set; }
}

public class LoginResponse
{
    [JsonPropertyName("csid")]
    public string SessionId { get; private set; }

    [JsonPropertyName("tsid")]
    public string TemporarySessionId { get; private set; }

    [JsonPropertyName("privk")]
    public string PrivateKey { get; private set; }

    [JsonPropertyName("k")]
    public string MasterKey { get; private set; }
}
#region Nodes

public class GetNodesRequest : RequestBase
{
    public GetNodesRequest()
        : base("f")
    {
        c = 1;
    }

    [JsonPropertyName("c")]
    public int c { get; private set; }
}

public class GetNodesResponse
{
    //public Node[] Nodes { get; private set; }

    [JsonPropertyName("f")]
    public Node[] Nodes { get; private set; }

    [JsonPropertyName("ok")]
    public SharedKey[] SharedKeys { get; private set; }
}

public class SharedKey
{
    [JsonPropertyName("h")]
    public string Id { get; private set; }

    [JsonPropertyName("k")]
    public string Key { get; private set; }
}

#region Attributes

public class Attributes
{
    public Attributes(string name)
    {
        Name = name;
    }

    [JsonPropertyName("n")]
    public string Name { get; set; }
}

#endregion

#region Create node

public class CreateNodeRequest : RequestBase
{
    private CreateNodeRequest(Node parentNode, NodeType type, string attributes, string key, string completionHandle)
        : base("p")
    {
        ParentId = parentNode.Id;
        Nodes = new[]
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

    [JsonPropertyName("t")]
    public string ParentId { get; private set; }

    [JsonPropertyName("n")]
    public CreateNodeRequestData[] Nodes { get; private set; }
}

public class CreateNodeRequestData
{
    [JsonPropertyName("h")]
    public string CompletionHandle { get; set; }

    [JsonPropertyName("t")]
    public NodeType Type { get; set; }

    [JsonPropertyName("a")]
    public string Attributes { get; set; }

    [JsonPropertyName("k")]
    public string Key { get; set; }
}

#endregion

#region Delete

public class DeleteRequest : RequestBase
{
    public DeleteRequest(Node node)
        : base("d")
    {
        Node = node.Id;
    }

    [JsonPropertyName("n")]
    public string Node { get; private set; }
}

#endregion

#region UploadRequest

internal class UploadUrlRequest : RequestBase
{
    public UploadUrlRequest(long fileSize)
        : base("u")
    {
        Size = fileSize;
    }

    [JsonPropertyName("s")]
    public long Size { get; private set; }
}

internal class UploadUrlResponse
{
    [JsonPropertyName("p")]
    public string Url { get; private set; }
}

#endregion

#region DownloadRequest

public class DownloadUrlRequest : RequestBase
{
    public DownloadUrlRequest(Node node)
        : base("g")
    {
        Id = node.Id;
    }

    [JsonPropertyName("g")]
    public int g { get { return 1; } set { } }

    [JsonPropertyName("n")]
    public string Id { get; private set; }
}

public class DownloadUrlResponse
{
    [JsonPropertyName("g")]
    public string Url { get; private set; }

    [JsonPropertyName("s")]
    public long Size { get; private set; }

    [JsonPropertyName("at")]
    private string SerializedAttributes { get; set; }
}

#endregion

#region Move

public class MoveRequest : RequestBase
{
    public MoveRequest(Node node, Node destinationParentNode)
        : base("m")
    {
        Id = node.Id;
        DestinationParentId = destinationParentNode.Id;
    }

    [JsonPropertyName("n")]
    public string Id { get; private set; }

    [JsonPropertyName("t")]
    public string DestinationParentId { get; private set; }
}

#endregion

#region SetAttributes

public class SetAttributesRequest : RequestBase
{
    public SetAttributesRequest(Node node, string serializedAttributes, string key)
        : base("a")
    {
        Id = node.Id;
        SerializedAttributes = serializedAttributes;
        Key = key;
    }

    [JsonPropertyName("n")]
    public string Id { get; private set; }

    [JsonPropertyName("at")]
    public string SerializedAttributes { get; private set; }

    [JsonPropertyName("k")]
    public string Key { get; private set; }
}

#endregion

#endregion
