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
    private Agent agent = null;

    private static string GetTestKey()
    {
      return File.ReadAllText("../testkey");
    }

    [SetUp]
    public void Init()
    {
      agent = new Agent(testKey);
    }

    [TearDown]
    public void Flush()
    {
      while(agent.MessageCount > 0)
        {
          System.Threading.Thread.Sleep(10);
        }
    }

    [Test]
    public void TestGauge()
    {
      agent.Gauge("csharp.TestGauge", 1.0f);
    }

    [Test]
    public void TestIncrement()
    {
      agent.Increment("csharp.TestIncrement");
    }

    [Test]
    public void TestTime()
    {
      agent.Time("csharp.TestTime", () => { System.Threading.Thread.Sleep(100); return 1; });
    }

    [Test]
    public void TimeReturnsActionResult()
    {
      var actionResult = 27;
      Func<int> action = () => { return actionResult; };
      Assert.AreEqual(actionResult, agent.Time("csharp.TestReturns", action));
      var actionResult2 = "things";
      Func<string> action2 = () => { return actionResult2; };
      Assert.AreEqual(actionResult2, agent.Time("csharp.TestReturns", action2));
      Assert.AreEqual(actionResult2, agent.Time("csharp.TestReturns", () => { return actionResult2; }));;
    }

    [Test]
    public void TestTimeMs()
    {
      agent.TimeMs("csharp.TestTimeMs", () => { System.Threading.Thread.Sleep(100); return 1; });
    }

    [Test]
    public void TestNotice()
    {
        var message = "C# test notice please ignore.";
        Assert.AreEqual(message, agent.Notice(message));
    }

    [Test]
    public void TestGaugeAtATime()
    {
      agent.Gauge("csharp.TestGaugePast", 1.0f, pastEventTime);
    }

    [Test]
    public void TestIncrementAtATime()
    {
      Assert.AreEqual(13, agent.Increment("csharp.TestIncrementPast", 13, pastEventTime));
    }

    [Test]
    public void DisabledIncrementReturnsValue()
    {
      agent.Enabled = false;
      Assert.AreEqual(13, agent.Increment("csharp.TestIncrementPast", 13, pastEventTime));
    }

    [Test]
    public void InvalidMetricNameReturnsNull()
    {
      Assert.AreEqual(null, agent.Increment(@"csharp.
TestIncrementPast", 13, pastEventTime));
    }

    [Test]
    public void TestNoticeAtATime()
    {
      agent.Notice("C# test notice FROM THE PAST using TimeSpan", pastEventTime, TimeSpan.FromMinutes(3));
    }

    [Test]
    public void TestEnabled()
    {
      // This test tests only that a message does not end up in the queue when agent is disabled
      // It is possible that the message was just enqueued and dequeued so quickly that the message count
      // was still zero
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
      int fasterThanYourNetwork = 5;

      var startTime = DateTime.Now;
      agent.Increment("csharp.BlockingTest");
      var duration = DateTime.Now - startTime;
      Assert.Less(duration.TotalMilliseconds, fasterThanYourNetwork);
    }

    [Test]
    //[ExpectedException("System.ArgumentException")] really should work here... /shrug
    public void TestBadApiKey()
    {
      bool excepted = false;
      try { agent = new Agent(null); }
      catch(ArgumentException) { excepted = true; }
      Assert.AreEqual(true, excepted);
    }

    [Test]
    public void TestGaugeWithCount()
    {
      agent.Gauge("csharp.GaugeWithCount", value: 1, count: 10);
    }

    [Test]
    public void TestIncrementWithCount()
    {
      agent.Increment("csharp.IncrementWithCount", value: 1, count: 10);
    }
  }
}
