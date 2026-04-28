# Zombie Land — Echoes of Memory

A small Unity / C# game built for the Spring 2026 Computer Animation
project. You play a wandering soul gathering glowing **Memory Fragments**
through a foggy maze patrolled by shambling zombies that hunt you with
**A* pathfinding** — but cannot harm you. The "Memory" theme is woven
through the gameplay, lighting, and UI.

## Quick start

```text
1. Install Unity 2022.3 LTS (Built-in Render Pipeline).
2. Open this folder in Unity Hub.
3. Create a new empty Scene.
4. Add an empty GameObject called "Bootstrap".
5. Attach Assets/Scripts/Managers/LevelBuilder.cs to it.
6. Press Play.
```

The entire scene (geometry, lights, fog, player, zombies, fragments,
exit, camera, UI) is generated procedurally from `LevelBuilder.cs` —
there is no scene asset to break.

## Controls

- **WASD** — move
- **F** — toggle flashlight
- **Esc** — pause / resume

## Repository layout

```
Assets/
  Scripts/
    Pathfinding/      A* algorithm and grid (Node, PathfindingGrid, AStarPathfinder)
    Player/           Player movement, flashlight, stats
    Enemy/            Zombie state machine + path follower
    Items/            Memory fragments and exit portal
    Environment/      MazeData (ASCII map)
    Managers/         GameManager (state) + LevelBuilder (procedural scene)
    UI/               HUDController, MenuController, UIBuilder
    Utility/          SmoothFollowCamera with built-in screen shake
  Scenes/             (you create your scene here)
  Prefabs/            (empty - everything is built at runtime)
  Resources/          (empty)
docs/
  ProjectDescription.md
  UserManual.md
  Innovation.md
```

## Documentation

See the `docs/` folder:

- [`docs/ProjectDescription.md`](docs/ProjectDescription.md) — what,
  why, where, how, and the rubric crosswalk.
- [`docs/UserManual.md`](docs/UserManual.md) — installation, controls,
  goal, tips.
- [`docs/Innovation.md`](docs/Innovation.md) — what's creative about
  the project and its inspirations.

## Tech notes

- **Language**: C# only. No JavaScript, UnityScript, or Boo.
- **Render pipeline**: Built-in. The procedural materials use the
  Standard shader, so HDRP / URP projects will need shader swaps.
- **AI technique**: A* pathfinding (`AStarPathfinder.cs`) on a 2D
  grid (`PathfindingGrid.cs`), driven by a small finite state
  machine (`Zombie.cs`).
- **Theme integration**: zombie contact does not damage the player;
  it triggers a "memory disturbance" effect (camera shake + HUD
  message). Fragments display poetic memory text on collection and
  are collected into a win-screen log.
```
# Zombie_Land_Unity
