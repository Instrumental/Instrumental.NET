### 1.2.0 [August 17, 2016]
* Make agent more correctly reject metric names with characters not supported by the service - [A-Za-z0-9] instead of \d\w
* Change Time() and TimeMs() to use more accurate counters to get more precise measurements with very fast functions

### 1.1.0 [August 8, 2016]
* Make agent string more consistent with other Instrumental agents

### 1.0.0 [May 9, 2016]
* Improvements in authentication mechanism
* Time() can now be used with Actions (which return void)
* MessageCount no longer reports 0 when a message in the process of being sent

### 0.2.0 [April 27, 2016]
* Modify API for most existing function to match other agents
* Make most Instrumental calls return the value being sent, or the action result for Time()
* Improve behavior on disconnect/reconnect

### 0.1.0 [April 13, 2016]
* Clean up existing build process

### 0.0.1 [April ??, 2016]
* Initial release
