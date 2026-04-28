# Innovation & Creativity

## What makes this project different

Most student "zombie" games are survival shooters: zombies chase you,
they damage you, you die. *Zombie Land — Echoes of Memory* deliberately
inverts that template.

- **The zombies cannot hurt you.** They are not enemies — they are
  *echoes of your own forgotten self*. When one passes through you the
  game shakes the camera and prints "*...a memory flickers...*" and
  that is the entire mechanical consequence of contact. This was a
  conscious design choice to align with the assignment's **"Memory"**
  theme: the player is not fighting a threat, they are reconstructing
  themselves. The danger is symbolic, not numeric.

- **The flashlight is a double-edged tool.** It pushes back the fog
  and lets you see further, but it also extends the radius at which
  zombies notice you. We use the same value (`PlayerFlashlight.On`)
  to drive both the spotlight intensity and the zombies' effective
  sight range — a tiny piece of system design that turns one input
  into two interacting systems.

- **The whole game world is generated from source code.** There is no
  prefab, no .unity scene asset, and no committed material in the
  repository — every wall, light, particle, button and material is
  created at runtime by `LevelBuilder.cs` and `UIBuilder.cs`. This
  makes the project trivially reproducible: drop a single empty
  GameObject with `LevelBuilder` into a fresh scene and press Play.
  It also means the entire game is reviewable as plain C# in
  pull-request diffs.

## Inspirations

- The lonely, fog-soaked ambience and "ghost in a forgotten place"
  framing are inspired by *Inside* (Playdead) and the early scenes of
  *Limbo*.
- The "collect memory shards to remember who you are" loop is
  reminiscent of *What Remains of Edith Finch* and *Gris*.
- The pathfinding implementation follows the classic A*-on-a-grid
  approach taught in introductory AI courses (and Sebastian Lague's
  free pathfinding lectures), reimplemented from scratch in
  `AStarPathfinder.cs` so the algorithm is fully visible — a single
  open list, octile heuristic (10 / 14), tie-break on lower hCost,
  diagonal corner-cutting prevented.

## Creative mechanics introduced

1. **Memory disturbance feedback** — the camera-shake-on-contact
   replaces traditional damage with a thematic, harmless tell.
2. **Flashlight visibility trade-off** — light helps you and hurts you.
3. **Inline narrative through pickups** — each fragment carries a
   single line of poetic text shown both as a transient HUD message
   and assembled into the win-screen "Remembered" log.
4. **Procedural-from-code world** — the whole project is a single
   pressable button in any clean scene; nothing is hand-baked.

## What I'd do next

- A simple ambient audio bed and a soft "ping" when a fragment is
  collected.
- Animator-driven shambling for the zombies (the script already feeds
  a `Speed` parameter into any Animator child it finds).
- A second level with a different maze layout — only `MazeData.cs`
  needs to change.
