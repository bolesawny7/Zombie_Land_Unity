# User Manual

## How to run the game

The project is a regular Unity project. To open and play it:

1. Install **Unity 2022.3 LTS** (or any newer LTS) using Unity Hub.
   Make sure the **Built-in Render Pipeline** module is included
   (it is by default for "3D Core" projects).
2. In Unity Hub click *Open* → *Add project from disk* → select the
   `ZombieLand` folder.
3. Open Unity. The project's Library folder will be regenerated on
   first open — wait for the import to finish.
4. In the Project window, create a new Scene
   (`Assets > Create > Scene`), name it `Main`, and open it.
5. Create an empty GameObject in the Hierarchy named `Bootstrap`.
6. Add the script `Assets/Scripts/Managers/LevelBuilder.cs` to it
   (drag and drop, or Inspector → *Add Component*).
7. Press **Play**. The entire world (floor, walls, lighting, fog,
   player, zombies, memory fragments, exit, camera, and UI) is built
   from code at runtime — no other scene setup is required.

> If you prefer to build a standalone executable, set the scene as
> the active build scene (`File > Build Settings`) and click *Build*.

## Controls

| Action     | Key            |
|------------|----------------|
| Move       | **WASD** / Arrow keys |
| Toggle flashlight | **F**   |
| Pause / unpause   | **Esc** |
| Confirm menu choice | **Mouse click** |

## Goal

- Find and walk into all **5 glowing blue Memory Fragments** scattered
  through the maze.
- After collecting them, walk into the warm **Light disc** at the
  bottom of the map. Walking in early will print how many fragments
  remain.
- A small message at the centre of the screen prints the text of each
  fragment as you collect it. The full list of remembered memories is
  shown on the win screen.

## Zombies

- Zombies wander the maze at random. If you get close, they switch
  into chase mode and pathfind toward you using **A***.
- Carrying your flashlight on increases the distance at which they
  notice you (your light gives you away).
- **They cannot hurt you.** When a zombie brushes through you, the
  camera shudders and the message *"...a memory flickers..."* appears.
  This is the only consequence of contact — the project is a
  meditative exploration game, not a survival game.

## Tips

- Conserve your flashlight battery if you want to stay hidden longer.
  The bar in the top-left ticks down while it's on.
- Pillars block sight lines but A* lets the zombies walk around them
  efficiently — don't assume they'll get stuck.
- The exit pulses with a warm light. It is always at the bottom-centre
  of the maze.
