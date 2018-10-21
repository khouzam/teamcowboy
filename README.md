# Team Cowboy .NETCore Library #

## Copyright (c) MagikInfo 2018 ##

### Licenced under the [MIT License](https://opensource.org/licenses/MIT)

This is a first implementation of a [Team Cowboy](https://teamcowboy.com) client library.

To use:
1. Instantiate a MagikInfo.TeamCowboy.TeamCowboyService with the public and private keys.
2. Login as the user.
3. Invoke APIs to query the state.

```C#
var service = new TeamCowboyService("PublicKey", "PrivateKey");
var token = service.LoginAsync("user", "password");
// Keep the token if you don't want to login every time
...
```

Build Status: ![Build Status](https://magikinfo.visualstudio.com/TeamCowboy/_apis/build/status/TeamCowboy-.NET%20Desktop-CI)