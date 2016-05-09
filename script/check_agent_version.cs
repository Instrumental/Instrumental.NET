#!/usr/bin/env csharp

LoadAssembly("bin/InstrumentalWithDependencies.dll")
Console.Out.Write(Instrumental.Agent.AgentVersion)
