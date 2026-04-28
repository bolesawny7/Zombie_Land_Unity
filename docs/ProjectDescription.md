# Project Description

## Zombie Land — Echoes of Memory

In **Zombie Land: Echoes of Memory** the player wakes as a wandering soul
in a foggy, ruined town that exists only inside their own forgotten past.
Five glowing **Memory Fragments** drift through the streets — each one a
splinter of who they used to be. The player explores the maze on foot
(WASD), shines their **flashlight (F)** through the fog to push back the
darkness, and gathers every fragment before walking into the **Light**
that marks the way out.

Why do they do it? Because the soul has no name yet. Each fragment they
collect prints a line of remembered text on the screen — the smell of
rain on a rooftop, the lullaby of a sister, the lantern of a lighthouse
keeper. The motivation is curiosity and self-recognition. The shambling
**zombies** that wander the alleys are *not* enemies in the traditional
sense — they are echoes of the player's own forgotten self. When they
brush against the player, the screen briefly shudders and a memory
"flickers", but the player cannot die. They are already a ghost. The
zombies are presences, not predators — they exist to make the world feel
populated, restless, and reluctant to let the player leave.

The aesthetic is night-blue and amber: thick exponential fog, a cool
moonlit ambient, scattered point lights from each memory orb, and a warm
spotlight from the flashlight. The mood is **lonely, melancholy, and
quietly hopeful** — closer to a walking-sim or *Inside* than to a
survival-horror game. By the time the player steps into the Light, the
HUD has stitched their fragments back into a small constellation of
memories — a reconstructed self.

This delivery satisfies all of the project rubric components:

| Rubric component        | Where it lives                                          |
|-------------------------|---------------------------------------------------------|
| Scene Building          | `LevelBuilder.cs` builds floor, walls, lights, props    |
| User Interface          | `UIBuilder.cs` + `MenuController.cs` + `HUDController.cs` (Main / HUD / Pause / Win) |
| Input Handling          | `PlayerController.cs` (WASD), `PlayerFlashlight.cs` (F), `GameManager.cs` (ESC pause) |
| Collision Detection     | `CharacterController` vs walls; trigger colliders on fragments, exit, and zombies |
| Lighting                | Directional moon + warm fill, point lights on orbs, spotlight flashlight, fog |
| Animation               | Bobbing/rotating memory fragments, pulsing exit glow, smooth camera follow + shake, smooth player rotation |
| Artificial Intelligence | A* pathfinding (`AStarPathfinder.cs`) + finite state machine (`Zombie.cs`) |
| Theme: Memory           | Core gameplay loop, fragment text, "memory disturbance" instead of damage |
