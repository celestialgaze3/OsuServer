# osu! Server
An osu!stable server implementation written in C#. This is my first (significant) C# project. 

### Features
- Packet handling (user info, online users, channels, dms)
- Accounts (in-game registration and login)
- Score submission (will save and appear on global leaderboards)

### Planned
- Per-mod/friend leaderboards
- Multiplayer
- Permission management
- In-game commands
- Performance points calculation (custom)
- Custom ranked maps (rank graveyard stuff)
#### Some things I may be interested in implementing one day:
- Web frontend
- Replay storage
- Custom profile pictures
- Separate profiles for relax scores
- Gamemodes other than standard

### Connect to the server
You can connect to this server by adding `-devserver celestialgaze.net/osu/server` to the end of the path of your osu! shortcut and running from there.
