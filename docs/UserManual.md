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
| Fire Light-Burst gun | **Left Mouse Button** |
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

The maze has three flavours of zombie. Each one wanders randomly
until it spots you, then switches into chase mode and pathfinds
toward you using **A\***. They are physically solid — they can no
longer phase through walls.

| Variant | Look | Speed | Notes |
|---------|------|-------|-------|
| **Walker** | Tall green-grey shambler | Slow | The default cannon-fodder zombie. |
| **Runner** | Lean red-tinted figure with bright crimson eyes | **Fast** | Sees further; will close gaps quickly. |
| **Brute**  | Big, dark, broad with amber eyes | Slow | Tankier silhouette; harder to dodge in narrow spaces. |

- Carrying your flashlight on increases the distance at which they
  notice you (your light gives you away).
- **They cannot hurt you.** When a zombie brushes through you, the
  camera shudders and the message *"...a memory flickers..."* appears.
  This is the only consequence of contact.

## The Light-Burst Gun

You carry a small "light-burst" gun fired with **Left Mouse Button**.

- It does not kill — instead it **stuns** any zombie it hits for
  ~3 seconds. Stunned zombies stop in place and tint blue.
- A crosshair in the centre of the screen shows where you are aiming.
- The beam is a hitscan raycast — there is no travel time, no
  ammunition limit, only a short cooldown between shots.
- Use it to peel runners off your back when you need a clean run to
  the next memory fragment.

## Tips

- Conserve your flashlight battery if you want to stay hidden longer.
  The bar in the top-left ticks down while it's on.
- Pillars block sight lines but A* lets the zombies walk around them
  efficiently — don't assume they'll get stuck.
- The exit pulses with a warm light. It is always at the bottom-centre
  of the maze.
