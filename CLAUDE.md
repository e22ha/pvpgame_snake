# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build
dotnet build client/console_snake.csproj
dotnet build server/server.csproj

# Run (start server first, then client)
dotnet run --project server/server.csproj
dotnet run --project client/console_snake.csproj
```

## Architecture

PVP multiplayer Snake game with a client-server TCP architecture. Both client and server are C#/.NET 5.0 console applications.

### Communication Protocol

String-delimited messages over TCP:
- `#<guid>/` — player registration
- `.<direction>.<guid>` — direction update (directions: up/down/left/right)
- `..<food_x>,<food_y>.<snake_body>|.<snake_body>|...` — server broadcasts game state each tick
- `/ping`, `/pong` — keep-alive (3.1s timeout)
- `/ready` — player ready to start
- `/play_eat` — server triggers eat sound on client
- `/win`, `/lose` — game end events

### Server (`server/`)

- `Server.cs` — TCP listener; one thread per client; dispatches parsed messages to `Game.cs`
- `Game.cs` — authoritative game state; 1-second tick timer; handles collision detection (wall/self/other snake), food spawning, win/lose logic
- `ClientInfo.cs` — per-client state: GUID, ready flag, ping timestamp
- `LobbyRoom.cs` / `LobbyList.cs` — lobby structures (implemented but not yet wired into game flow)

### Client (`client/`)

- `Client.cs` — TCP connection; receives game state; fires events (`DataForUpdate`, `PlaySound`, `IamWin`, `IamLose`)
- `Game.cs` — parses server broadcast and renders the 45×45 console grid
- `Snake.cs` — local snake entity: body as list of points, movement, self-collision check
- `AlternateConsole.cs` — named pipe to a second console window used as a message/log display
- `Sounds.cs` — wraps WAV playback (Windows only via `System.Windows.Extensions`)

### Game Grid

45×45 console window; `Walls.cs` draws the border. Food rendered as `@`. Server is authoritative for all positions.
