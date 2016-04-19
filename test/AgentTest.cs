namespace Instrumental
{
  using System;
  using System.IO;
  using NUnit.Framework;


  [TestFixture]
  public class AgentTest
  {
    private static DateTime pastEventTime = DateTime.Now.AddMinutes(-15);
    private static string testKey = GetTestKey();

    private static string GetTestKey()
    {
      return File.ReadAllText("../testkey");
    }

    [Test]
    public void TestGauge()
    {
      var agent = new Agent(testKey);
      agent.Gauge("csharp.TestGauge", 1.0f);
    }

    [Test]
    public void TestIncrement()
    {
      var agent = new Agent(testKey);
      agent.Increment("csharp.TestIncrement");
    }

    [Test]
    public void TestTime()
    {
      var agent = new Agent(testKey);
      agent.Time("csharp.TestTime", () => { System.Threading.Thread.Sleep(100); });
    }

    [Test]
    public void TestTimeMs()
    {
      var agent = new Agent(testKey);
      agent.TimeMs("csharp.TestTimeMs", () => { System.Threading.Thread.Sleep(100); });
    }

    [Test]
    public void TestNotice()
    {
      var agent = new Agent(testKey);
      agent.Notice("C# test notice please ignore.");
    }

    [Test]
    public void TestGaugeAtATime()
    {
      var agent = new Agent(testKey);
      agent.Gauge("csharp.TestGaugePast", 1.0f, pastEventTime);
    }

    [Test]
    public void TestIncrementAtATime()
    {
      var agent = new Agent(testKey);
      agent.Increment("csharp.TestIncrementPast", 13, pastEventTime);
    }

    [Test]
    public void TestNoticeAtATime()
    {
      var agent = new Agent(testKey);
      agent.Notice("C# test notice FROM THE PAST please ignore.", 300, pastEventTime);
    }

    [Test]
    public void TestEnabled()
    {
      // This test tests only that a message does not end up in the queue when agent is disabled
      // It is possible that the message was just enqueued and dequeued so quickly that the message count
      // was still zero
      var agent = new Agent(testKey);
      agent.Enabled = false;
      agent.Increment("csharp.YouShouldNotSeeThisMetric");
      Assert.AreEqual(0, agent.MessageCount, "Disabled agent still queued a message");
    }

    [Test]
    public void TestNonBlocking()
    {
      // This might be blocking still - we just test that the time it takes to return is very low
      // It is an unacknowledge protocol, so it might be that that fast
      // When agent configuration is possible, testing this with a misconfigured agent may be better
      var agent = new Agent(testKey);
      int fasterThanYourNetwork = 5;

      var startTime = DateTime.Now;
      agent.Increment("csharp.BlockingTest");
      var duration = DateTime.Now - startTime;
      Assert.Less(duration.TotalMilliseconds, fasterThanYourNetwork);
    }

    [Test]
    public void TestBadApiKey()
    {
      var agent = new Agent("if this is a valid key, something has gone terribly wrong");
      agent.Increment("csharp.BadApiKey");
      int exceptionsThrown = 0;
      try
        {
          agent = new Agent(null);
        }
      catch(ArgumentException e)
        {
          exceptionsThrown += 1;
        }
      Assert.AreEqual(1, exceptionsThrown);
    }
  }
}
