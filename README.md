Instrumental .NET Agent
================

Instrumental is a [application monitoring platform](https://instrumentalapp.com) built for developers who want a better understanding of their production software. Powerful tools, like the [Instrumental Query Language](https://instrumentalapp.com/docs/query-language), combined with an exploration-focused interface allow you to get real answers to complex questions, in real-time.

This agent supports custom metric monitoring for .NET applications.

Getting Started
===============

If you are using NuGet, add Instrumental.NET to your packages.config:

```xml
<id="Instrumental" version="1.0.0" targetFramework="net45" />
```

Or, download [Instrumental.dll](https://github.com/Instrumental/instrumental_agent-csharp/releases/latest).

Visit [instrumentalapp.com](https://instrumentalapp.com) and create an account, then initialize the agent with your API token, found in the Docs section.

Simple Example
==============

Here are the basic Instrumental monitoring commands:

```csharp
using Instrumental;

var agent = new Agent("project api token here");

agent.Increment("myapp.logins");
agent.Gauge("myapp.server1.free_ram", 1234567890);
agent.Notice("Server maintenance window", TimeSpan.FromMinutes(15));
Func<string> action = () => { DoLongRunningAction(); return "everything is fine"; };
String actionResult = agent.Time("myapp.expensive_operation", action);
```

Worker Example
==============

You can easily use Instrumental Agent with background workers too, in this somewhat contrived timing example:

```csharp
using Instrumental;
using System.ComponentModel;

BackgroundWorker bg = new BackgroundWorker();
bg.DoWork += delegate { agent.Time("csharp.worker.TimedWorker", () => { System.Threading.Thread.Sleep(500); return 0;} );
bg.RunWorkerAsync();
```

Links
=====

[Instrumental](http://instrumentalapp.com)
[Common.logging](http://netcommon.sourceforge.net/)
