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

namespace Instrumental
{
  /// <summary>
  ///   Extensions to core classes to make our lives easier
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    ///   The time at the start of the Unixverse.
    /// </summary>
    public static readonly DateTime EpochStart = new DateTime(1970, 1, 1);

    /// <summary>
    ///   Convert a DateTime to the number of the seconds since the Epoch
    /// </summary>
    public static int ToEpoch(this DateTime dt)
    {
      return (int)(dt.ToUniversalTime() - EpochStart).TotalSeconds;
    }
  }
}
