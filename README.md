Instrumental .NET Agent
================

Instrumental is a [application monitoring platform](https://instrumentalapp.com) built for developers who want a better understanding of their production software. Powerful tools, like the [Instrumental Query Language](https://instrumentalapp.com/docs/query-language), combined with an exploration-focused interface allow you to get real answers to complex questions, in real-time.

This agent supports custom metric monitoring for .NET applications.


Features
========
 - Doesn't require Statsd
 - Uses [Common.logging], so it probably works with your existing logging library

Example
=======
```C#
using Instrumental.NET;

var agent = new Agent("your api key here");

agent.Increment("myapp.logins");
agent.Gauge("myapp.server1.free_ram", 1234567890);
agent.Time("myapp.expensive_operation", () => LongRunningOperation());
agent.Notice("Server maintenance window", 3600);
```

[Instrumental]:http://instrumentalapp.com
[Common.logging]:http://netcommon.sourceforge.net/
