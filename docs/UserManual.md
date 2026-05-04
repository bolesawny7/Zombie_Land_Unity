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
| Detonate bomb     | **Space** |
| Pause / unpause   | **Esc** |
| Confirm menu choice | **Mouse click** |

The HUD shows three bars in the top-left:

1. **Pink Soul Integrity** bar — your health. Drops every time a
   zombie touches you, and slowly regenerates after a short delay.
   If it reaches zero, the screen fades to a *"The fog took you"*
   panel and you can hit *Try Again* to restart the run.
2. **Amber Flashlight Battery** bar — drains while the flashlight is on.
3. **Blue Sprint Stamina** bar — drains while sprinting (turns lighter
   green-white at the moment you're actually sprinting), regenerates
   while not.

The top-right shows your **bomb count** — the number of detonators
you currently carry.

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
- Each contact with a zombie chips away at your **Soul Integrity**
  bar and shudders the camera. If it hits zero, you lose.

## Bombs

Two **bomb pickups** (small spinning red orbs with a fuse) are
scattered through the maze. Walking into one adds it to your inventory
(top-right counter). Press **Space** at any time to detonate one — a
bright orange shockwave expands around you and **kills any zombie
caught in the blast**, including Brutes. Use them on rooms full of
zombies, or save one for the Runner that's been chasing you.

## Tips

- A **Runner** can outrun your normal walk speed; you'll need to
  **sprint** to break line of sight, then duck behind a pillar and
  let it lose interest before your stamina runs out.
- The **Brute** is slow but turns sharply — never sprint *toward* a
  brute at a corner; you'll burn stamina you may need to sprint *away*.
- Memory fragments emit a soft blue point light — they're often
  visible through fog before the orb itself becomes clear.
- A bomb on the ground in the middle of the map is the easiest one
  to grab on the way back; pick it up early and you'll always have
  a panic button.

## Tips

- Conserve your flashlight battery if you want to stay hidden longer.
  The bar in the top-left ticks down while it's on.
- Pillars block sight lines but A* lets the zombies walk around them
  efficiently — don't assume they'll get stuck.
- The exit pulses with a warm light. It is always at the bottom-centre
  of the maze.
