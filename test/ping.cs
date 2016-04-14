namespace Instrumental
{

  using NUnit.Framework;

  [TestFixture]
  public class AgentTest
  {

    [Test]
    public void SmokeOk()
    {
      Assert.AreEqual(1, 1);
    }

    [Test]
    public void SmokeNotOk()
    {
      Assert.AreEqual(true, false);
    }
  }
}
