# Innovation & Creativity

## What makes this project different

Most student "zombie" games are survival shooters with one verb:
shoot. *Zombie Land — Echoes of Memory* keeps the haunting "ghost in
a forgotten place" framing but pairs it with two contrasting verbs —
a passive **sprint** for evasion, and an active **bomb** for
catharsis — so traversal is always a meaningful decision.

- **A "Soul Integrity" health bar instead of generic HP.** Each
  contact with a zombie chips it down; staying away regenerates it.
  When it hits zero you don't get a "Game Over" — you see a
  *"The fog took you"* panel and a *Try Again* button. The framing
  stays in the project's memory theme even when the mechanics are
  classic survival.

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

1. **Soul-integrity health & death loop** — a regenerating health bar
   keeps small bumps cheap but makes a sustained chase fatal.
2. **Flashlight visibility trade-off** — light helps you and hurts you.
3. **Sprint with stamina** — the player has a clean evasion verb
   (Left Shift) but must manage a limited resource. Stamina regenerates
   only while not sprinting, so the player has to commit to bursts
   rather than sprinting indefinitely.
4. **Bomb pickup powerup** — collectible orbs in the maze grant a
   single Space-bar shockwave that kills any zombie in radius. Killed
   zombies play a procedural collapse-and-shrink death animation.
5. **Three zombie variants from one script** — Walker, Runner, and
   Brute share `Zombie.cs`; the variant just tweaks speed, sight,
   colour, and silhouette parameters. Cheap to author, easy to extend.
6. **Procedural locomotion animation, no rigging required** —
   `ZombieAnimator` and `PlayerAnimator` synthesise body sway, head
   bob, arm swing, and a sprint forward-lean from the world-space
   movement of the parent transform. The whole game animates without
   any imported FBX rigs or Animator Controllers.
7. **Inline narrative through pickups** — each fragment carries a
   single line of poetic text shown both as a transient HUD message
   and assembled into the win-screen "Remembered" log.
8. **Procedural-from-code world** — the whole project is a single
   pressable button in any clean scene; nothing is hand-baked.

## What I'd do next

- A simple ambient audio bed and a soft "ping" when a fragment is
  collected; a heavy whoomph for the bomb.
- A second level with a different maze layout — only `MazeData.cs`
  needs to change.
- A throwable variant of the bomb (right-click to launch instead of
  detonating in-place).
