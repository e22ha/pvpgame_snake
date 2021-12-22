using NUnit.Framework;
using console_snake;
using System;

namespace TestClient;
[TestFixture]
public class Tests
{
    private Sounds cs;
    private Client p;

    [SetUp]
    public void Setup1()
    {
        cs = new Sounds();
        Sounds.fullPath = ".wav/icon_06.vaw";
    }

    [Test]
    public void Test1()
    {
        var exception = Assert.Throws<ArgumentException>(() => Sounds.EatSound());
        Assert.That(exception.Message, Is.EqualTo("not wav"));
    }

    [SetUp]
    public void Setup2()
    {
        p = new Client();
    }

    [TestCase("127.0.0.1", 8888)]
    public void Test2(string adress, int port)
    {
        string g = new Guid().ToString(); 
        //var exception = Assert.Throws<ArgumentException>(() => p.connect_(adress, port, g));
        //Assert.That(exception, Is.EqualTo(""));
        Assert.DoesNotThrow(() => p.connect_(adress, port, g));

    }
}
