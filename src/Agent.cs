//  Copyright 2014 Bloomerang
//  Copyright 2016 Expected Behavior, LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Text.RegularExpressions;
using Common.Logging;
using System.Diagnostics;

namespace Instrumental
{
  /// <summary>
  ///   Instrumental Agent used to send performance and other metrics to http://instrumentalapp.com
  /// </summary>
  public class Agent
  {
    /// <summary>
    ///   The agent version, according to the agent.
    /// </summary>
    public static readonly String AgentVersion = "1.1.0";

    /// <summary>
    ///   Enable/disable the agent.  This enables you to block sending of metrics to Instrumental without having to change your code.
    /// </summary>
    public bool Enabled { get; set; }

    private readonly Collector _collector;
    private static readonly ILog _log = LogManager.GetCurrentClassLogger();
    private readonly Regex _validateMetric;

    /// <summary>
    ///   The number of messages waiting to be sent to Instrumental
    /// </summary>
    public int MessageCount
    {
      get
        {
          return _collector.MessageCount;
        }
    }

    /// <summary>
    ///   Create a new Agent
    /// </summary>
    /// <param name="apiKey">Your project API key from instrumentalapp.com</param>
    public Agent(String apiKey)
    {
      if(string.IsNullOrEmpty(apiKey))
        throw new ArgumentException("api key was null or missing", apiKey);

      Enabled = true;
      _collector = new Collector(apiKey);

      var validationOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture;
      _validateMetric = new Regex(@"^([a-zA-Z0-9\-_]+\.)*[a-zA-Z0-9\-_]+$", validationOptions);

    }

    /// <summary>
    ///   Measure the average value of something in your code, like response time.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="value">The value to record.</param>
    /// <param name="time">The exact time at which the event happened.  You probably want the default.</param>
    /// <param name="count">The number of events which this represents.  You almost certainly want the default.</param>
    /// <returns>The value being passed in via value, or null if something bad happened.</returns>
    public double? Gauge(String metricName, double value, DateTime? time = null, int count = 1)
    {
      try
        {
          if(!ValidateMetricName(metricName)) return null;
          if(!Enabled) return value;
          int metricTime = (time ?? DateTime.Now).ToEpoch();
          _collector.SendMessage(String.Format("gauge {0} {1} {2} {3}", metricName, value, metricTime, count));
          return value;
        }
      catch (Exception e)
        {
          ReportException(e);
        }
      return null;
    }

    /// <summary>
    ///   Measure the duration of an action in seconds.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="action">The action being measured</param>
    /// <returns>The result of executing the action.</returns>
    public T Time<T>(String metricName, Func<T> action)
    {
      return ActuallyTime(metricName, action);
    }

    /// <summary>
    ///   Measure the duration of an action in seconds.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="action">The action being measured</param>
    public void Time(String metricName, Action action)
    {
      ActuallyTime(metricName, action);
    }

    /// <summary>
    ///   Measure the duration of an action in milliseconds.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="action">The action being measured</param>
    /// <returns>The result of executing the action.</returns>
    public T TimeMs<T>(String metricName, Func<T> action)
    {
      return ActuallyTime(metricName, action, 1000);
    }

    /// <summary>
    ///   Measure the duration of an action in milliseconds.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="action">The action being measured</param>
    public void TimeMs(String metricName, Action action)
    {
      ActuallyTime(metricName, action, 1000);
    }

    private T ActuallyTime<T>(String metricName, Func<T> action, double durationMultiplier = 1)
    {
      var timer = new Stopwatch();
      timer.Start();
      try
        {
          return action();
        }
      finally
        {
          timer.Stop();
          var duration = timer.Elapsed;
          Gauge(metricName, duration.TotalSeconds * durationMultiplier);
        }
    }

    private void ActuallyTime(String metricName, Action action, double durationMultiplier = 1)
    {
      var timer = new Stopwatch();
      try
        {
          action();
        }
      finally
        {

          timer.Stop();
          var duration = timer.Elapsed;
          Gauge(metricName, duration.TotalSeconds * durationMultiplier);
        }
    }

    /// <summary>
    ///   Measure an occurance of something in your code.
    /// </summary>
    /// <param name="metricName">The name of the metric to send.  Letters, numbers, and dots.</param>
    /// <param name="value">The value to record.</param>
    /// <param name="time">The exact time at which the event happened.  You probably want the default.</param>
    /// <param name="count">The number of events which this represents.  You almost certainly want the default.</param>
    /// <returns>The value being passed in via value, or null if something bad happened.</returns>
    public double? Increment(String metricName, double value = 1, DateTime? time = null, int count = 1)
    {
      try
        {
          if(!ValidateMetricName(metricName)) return null;
          if (!Enabled) return value;
          int metricTime = (time ?? DateTime.Now).ToEpoch();
          _collector.SendMessage(String.Format("increment {0} {1} {2} {3}", metricName, value, metricTime, count));
          return value;
        }
      catch (Exception e)
        {
          ReportException(e);
        }
      return null;
    }

    /// <summary>
    ///   Tag a point or duration in time with a note, like a deploy or a service outage.
    /// </summary>
    /// <param name="message">The note you want to show up in your metrics.  A sentence or less, no newlines.</param>
    /// <param name="time">The exact time at which the event happened.  You probably want the default.</param>
    /// <param name="duration">The amount of time the event took.</param>
    /// <returns>The message being passed in via message, or null if something bad happened.</returns>
    public String Notice(String message, DateTime? time = null, TimeSpan? duration = null)
    {
      try
        {
          if (!ValidateNote(message)) return null;
          if (!Enabled) return message;
          int metricTime = (time ?? DateTime.Now).ToEpoch();
          _collector.SendMessage(String.Format("notice {0} {1} {2}", metricTime, duration?.TotalSeconds ?? 0, message));
          return message;
        }
      catch (Exception e)
        {
          ReportException(e);
        }
      return null;
    }

    private static bool ValidateNote(String message)
    {
      var valid = message.IndexOf("\r") == -1 && message.IndexOf("\n") == -1;
      if(!valid) _log.WarnFormat("Invalid notice message: {0}", message);
      return valid;
    }

    private bool ValidateMetricName(String metricName)
    {
      if (_validateMetric.IsMatch(metricName))
        return true;

      Increment("agent.invalid_metric");
      _log.WarnFormat("Invalid metric name: {0}", metricName);

      return false;
    }

    private void ReportException(Exception e)
    {
      _log.Error("An exception occurred", e);
    }
  }
}
