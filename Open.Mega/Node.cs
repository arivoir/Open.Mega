namespace Open.Mega;

public class Node
{
    public static readonly DateTime OriginalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

    [JsonPropertyName("h")]
    public string Id { get; private set; }
    [JsonPropertyName("p")]
    public string ParentId { get; set; }
    [JsonPropertyName("u")]
    public string Owner { get; private set; }
    [JsonPropertyName("t")]
    public NodeType Type { get; private set; }
    [JsonPropertyName("s")]
    public long Size { get; private set; }
    [JsonPropertyName("a")]
    public string SerializedAttributes { get; set; }
    [JsonPropertyName("k")]
    public string SerializedKey { get; set; }
    [JsonPropertyName("ts")]
    public long SerializedLastModificationDate { get; set; }
    [JsonPropertyName("fa")]
    public string FileAttributes { get; set; }

    public string Name { get; set; }
    public DateTime LastModificationDate { get; set; }
    public byte[] Iv { get; set; }
    public byte[] MetaMac { get; set; }
    public byte[] Key { get; set; }
}

public enum NodeType
{
    File = 0,
    Directory = 1,
    Root = 2,
    Inbox = 3,
    Trash = 4,
}
