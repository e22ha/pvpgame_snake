<h1 align="center">🐍 PVP Snake</h1>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-5.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" />
  <img src="https://img.shields.io/badge/type-console-black?style=for-the-badge" />
</p>

<p align="center">
  A real-time multiplayer Snake game played in the Windows console. <br/>
  Two players connect over TCP and battle on a shared 45×45 grid — last snake standing wins.
</p>

<p align="center">
  📚 <em>First-year programming course project</em>
</p>

---

## Preview

```
#############################################
#                                           #
#                    @                      #
#                                           #
#   ***>                                    #
#                                           #
#                        <***               #
#                                           #
#                                           #
#############################################

  Player 1 [3e2a]  score: 10      Player 2 [7f1b]  score: 30
```

> Two console windows open automatically: the **game field** and a **message log**.

---

## Features

- **Real-time PVP** — two or more players connect to a shared server and play simultaneously
- **Collision detection** — wall hits, self-collisions, and snake-on-snake kills
- **Live food spawning** — food (`@`) respawns randomly after each eat
- **Sound effects** — eat sound plays on the winning player's client (Windows only)
- **Leaderboard** — winner submits their name to the leaderboard at the end of the game
- **Keep-alive** — ping/pong mechanism (3.1 s timeout) automatically drops disconnected clients
- **Dual-window UI** — game field and message log run in separate console windows via named pipes

---

## Architecture

```
┌─────────────────────────────────────────────┐
│                   SERVER                    │
│                                             │
│  TcpListener → thread-per-client            │
│  Game loop (1 s tick)                       │
│  Authoritative state: snakes + food         │
│  Broadcasts field state to all clients      │
└───────────────┬─────────────────────────────┘
                │ TCP  (127.0.0.1:8888)
       ┌────────┴────────┐
       ▼                 ▼
  ┌─────────┐       ┌─────────┐
  │Client 1 │       │Client 2 │
  │         │       │         │
  │ renders │       │ renders │
  │  field  │       │  field  │
  └─────────┘       └─────────┘
```

**Message protocol** (string over TCP, Unicode):

| Direction        | Format                                           | Meaning                 |
|------------------|--------------------------------------------------|-------------------------|
| client → server  | `#<guid>/`                                       | Register player         |
| client → server  | `.<UpArrow\|DownArrow\|LeftArrow\|RightArrow>.<guid>` | Direction update   |
| client → server  | `/ready`                                         | Player ready to start   |
| client → server  | `/pong`                                          | Keep-alive reply        |
| server → client  | `..<fx>,<fy>.<x,y\|x,y\|…>.<x,y\|…>`            | Full game state per tick|
| server → client  | `/play_eat`                                      | Trigger eat sound       |
| server → client  | `/win` / `/lose`                                 | Game over               |
| server → client  | `/ping`                                          | Keep-alive probe        |

---

## Getting Started

### Prerequisites

- [.NET 5 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)
- Windows (audio playback via `System.Windows.Extensions`)

### Build

```bash
dotnet build server/server.csproj
dotnet build client/console_snake.csproj
```

### Run

Start the **server** first:

```bash
dotnet run --project server/server.csproj
```

Then launch the **client** on each player's machine:

```bash
dotnet run --project client/console_snake.csproj
```

On startup the client will ask for the server IP and port:

```
IP?...         → enter server IP (default: 127.0.0.1)
Port?...       → enter port      (default: 8888)
Are you read(y)? → press Y to join the game
```

> Both players must press **Y** before the game loop starts.

---

## How to Play

| Key | Action |
|-----|--------|
| ← → ↑ ↓ | Steer your snake |

- Run over `@` to eat food — your snake grows and earns points
- Avoid the walls (`#`), your own body, and the other snake
- The last snake alive wins; the winner submits their name to the leaderboard

---

## Project Structure

```
pvpgame_snake/
├── client/
│   ├── Program.cs          — entry point, input loop, event handlers
│   ├── Client.cs           — TCP client, event-based message dispatch
│   ├── Game.cs             — renders the game field from server state
│   ├── Snake.cs            — local snake entity (movement, collision)
│   ├── AlternateConsole.cs — named-pipe secondary console window
│   ├── Sounds.cs           — WAV playback wrapper
│   └── Walls.cs            — border drawing
└── server/
    ├── Program.cs          — wires Server ↔ Game events
    ├── Server.cs           — TCP listener, per-client threads
    ├── Game.cs             — authoritative game loop, collision logic
    ├── ClientInfo.cs       — per-client state (GUID, ready flag, ping)
    ├── LobbyRoom.cs        — room / player list
    └── LobbyList.cs        — lobby registry
```

---

## Built With

- **C# 10 / .NET 5** — language and runtime
- **System.Net.Sockets** — TCP networking
- **System.IO.Pipes** — named pipes for the secondary console window
- **System.Windows.Extensions** — WAV audio playback

---

---

> **Note:** [Claude Code](https://claude.ai/code) AI assistant was used once to clean up comments, translate them to English, and fix several bugs (operator precedence in spawn positions, a broken broadcast condition, thread pool misuse, and a buffer overflow). All game logic and architecture were written by the authors.

<p align="center">Made with ♥ by <a href="https://github.com/e22ha">e22ha</a> & <a href="https://github.com/Lstafff">Lstafff</a></p>
