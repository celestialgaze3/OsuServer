# osu! Server
An osu!stable server implementation written in C#. This is my first (significant) C# project. 

### Features
- Packet handling (user info, online users, channels, dms)
- Accounts (in-game registration and login)
- Score submission (will save replays and appear on all leaderboard types)
- Separate profile and leaderboards for relax scores
- Multiplayer

### Planned
- Performance points calculation (custom)
#### Some things I might be interested in implementing one day:
- Custom profile pictures
- Spectating
- In-game commands
- Permission management
- Full support for gamemodes other than standard
- Custom ranked maps
- Web frontend

### Connect to the server
You can connect to this server by adding `-devserver celestialgaze.net/osu/server` to the end of the path of your osu! shortcut and running from there.
