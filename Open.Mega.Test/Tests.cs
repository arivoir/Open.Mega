using DotNetEnv;
using System.Text;

namespace Open.Mega.Test;
public class Tests
{
    private string _user;
    private string _password;
    private string _rootFolderId;

    [SetUp]
    public async Task Setup()
    {
        Env.Load();
        _user = Environment.GetEnvironmentVariable("USER")!;
        _password = Environment.GetEnvironmentVariable("PASSWORD")!;

        var client = new MegaClient();
        await client.Login(_user, _password, CancellationToken.None);
        var rootFolderName = Guid.NewGuid().ToString();
        _rootFolderId = rootFolderName;
        var node = new Node() { Name = rootFolderName };
        await client.CreateFolder("", node, CancellationToken.None);
    }

    [TearDown]
    public async Task TearDown()
    {
        var client = new MegaClient();
        await client.Login(_user, _password, CancellationToken.None);
        var node = new Node() { ParentId = "", Name = _rootFolderId };
        await client.Delete(node, CancellationToken.None);
    }

    [Test]
    public async Task UploadTest()
    {
        var client = new MegaClient();
        await client.Login(_user, _password, CancellationToken.None);
        var parent = new Node() { ParentId = "", Name = _rootFolderId };
        var stringToUpload = "Hello, World!";
        var filePath = Path.Combine(_rootFolderId, "file.txt");
        await client.Upload(new MemoryStream(Encoding.UTF8.GetBytes(stringToUpload)), "file.txt", parent, null, CancellationToken.None);
        var nodes = await client.GetNodes(CancellationToken.None);
        var files = nodes.Where(n => n.ParentId == _rootFolderId).ToArray();
        Assert.That(files, Is.Not.Null);
        Assert.That(files.Length, Is.EqualTo(1));
        Assert.That(files[0].Name, Is.EqualTo("file.txt"));
    }

}
