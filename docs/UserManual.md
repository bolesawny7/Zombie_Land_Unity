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
| Sprint     | **Hold Left Shift** (drains stamina) |
| Toggle flashlight | **F**   |
| Pause / unpause   | **Esc** |
| Confirm menu choice | **Mouse click** |

The HUD shows two bars in the top-left: an **amber Flashlight Battery**
bar and a **blue Sprint Stamina** bar. Sprinting drains stamina; the
bar regenerates as soon as you release Shift or stop moving.

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

## Tips

- A **Runner** can outrun your normal walk speed; you'll need to
  **sprint** to break line of sight, then duck behind a pillar and
  let it lose interest before your stamina runs out.
- The **Brute** is slow but turns sharply — never sprint *toward* a
  brute at a corner; you'll burn stamina you may need to sprint *away*.
- Memory fragments emit a soft blue point light — they're often
  visible through fog before the orb itself becomes clear.

## Tips

- Conserve your flashlight battery if you want to stay hidden longer.
  The bar in the top-left ticks down while it's on.
- Pillars block sight lines but A* lets the zombies walk around them
  efficiently — don't assume they'll get stuck.
- The exit pulses with a warm light. It is always at the bottom-centre
  of the maze.
