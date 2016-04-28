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

namespace Instrumental
{
  public class Agent
  {
    public bool Enabled { get; set; }

    private readonly Collector _collector;
    private static readonly ILog _log = LogManager.GetCurrentClassLogger();
    private readonly Regex _validateMetric;

    public int MessageCount
    {
      get
        {
          return _collector.MessageCount;
        }
    }

    public Agent(String apiKey)
    {
      if(string.IsNullOrEmpty(apiKey))
        throw new ArgumentException("api key was null or missing", apiKey);

      Enabled = true;
      _collector = new Collector(apiKey);

      var validationOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture;
      _validateMetric = new Regex(@"^([\d\w\-_]+\.)*[\d\w\-_]+$", validationOptions);
    }

    public float? Gauge(String metricName, float value, DateTime? time = null, int count = 1)
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

    public T Time<T>(String metricName, Func<T> action)
    {
      return ActuallyTime(metricName, action);
    }

    private T ActuallyTime<T>(String metricName, Func<T> action, float durationMultiplier = 1)
    {
      var start = DateTime.Now;
      try
        {
          return action();
        }
      finally
        {
          var end = DateTime.Now;
          var duration = end - start;
          Gauge(metricName, (float)duration.TotalSeconds * durationMultiplier);
        }
    }

    public T TimeMs<T>(String metricName, Func<T> action)
    {
      return ActuallyTime(metricName, action, 1000);
    }

    public float? Increment(String metricName, float value = 1, DateTime? time = null, int count = 1)
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
