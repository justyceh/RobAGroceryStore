# CLAUDE.md — Grocery Store Heist (Unity 3D)

## About me (the developer)
- I'm a software engineering intern comfortable with JavaScript/TypeScript, React, and Node, but **I'm new to Unity and the Unity Editor**. I have only a tiny bit of Unity experience.
- I'm building this game solo, mostly with your help writing code. I want to **understand** what we build, not just copy it.
- Assume I don't know where things are in the editor. Never say "just attach it" or "set it up as usual." Spell out every click.

## The game
A 3D game where the player sneaks into a grocery store, steals items, and must escape while dodging security guards who chase them.

**Core loop:** enter store → grab loot → guards detect and chase → reach the exit with loot (win) or get caught (lose).

**Camera perspective:** NOT DECIDED YET. Ask me before writing any camera or player-look code.

## Tech stack
- **Unity 6** with the **Universal Render Pipeline (URP)**. Never give Built-In pipeline shaders or materials. In URP those render pink.
- **C#** scripts only. No visual scripting.
- **Input System package** (the new one). Do NOT use the old `Input.GetKey` / `Input.GetAxis` API.
- **Cinemachine** for cameras.
- **ProBuilder** for blocking out the store level (graybox).
- **AI Navigation package** (NavMesh) for guard pathfinding.
- **Git + GitHub with Git LFS** for version control.

## Project structure
Put files only in these folders under `Assets/`:
- `Scripts/`: all C# scripts. Use subfolders as needed: `Player/`, `Guards/`, `Loot/`, `UI/`, `Game/`.
- `Prefabs/`: reusable GameObjects (guard, loot item, etc.)
- `Materials/`
- `Scenes/`
- `Models/`

**Never edit or create files in** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, or `ProjectSettings/` unless I explicitly ask. **Never create, edit, or delete `.meta` files.** Unity manages those.

## Build roadmap (do these in order, one at a time)
1. **Player movement:** a capsule that walks around a flat plane.
2. **Graybox store:** walls and aisles built with ProBuilder. This is mostly editor work, so guide me through it.
3. **Stealing:** walk up to an item, press a key, the item disappears, and a loot counter goes up.
4. **One guard:** a capsule that chases the player using NavMesh.
5. **Win/lose:** a guard touching the player means caught (lose). Reaching the exit with loot means escaped (win).

After those work: polish (art, multiple guards, guard vision cones, alarms, score, UI, sound).

Do **not** jump ahead to later milestones or add features I didn't ask for.

## Coding conventions
- One responsibility per script (e.g. `PlayerMovement`, `LootPickup`, `GuardChase`, `GameManager`).
- Use `[SerializeField] private` fields for anything I should tune in the Inspector, with sensible default values and a `[Tooltip]` explaining each one.
- Add short comments explaining *why*, especially for Unity-specific things (`Update` vs `FixedUpdate`, `Time.deltaTime`, physics callbacks, etc.).
- Don't use `GameObject.Find` or `GetComponent` inside `Update`. Cache references in `Awake`/`Start`.
- Keep scripts small and readable. Prefer clear code over clever code.
- Make sure code compiles. If you're unsure about a Unity 6 API, say so instead of guessing.

## How to work with me
- Work on **one feature per request**. If my request is big, propose breaking it into steps first.
- Before writing code, briefly tell me your plan (2–4 sentences).
- After writing code, explain in plain language what each script does and how the pieces connect. Relate Unity concepts to web dev when it helps (e.g. components ≈ composable modules, `Start()` ≈ a mount lifecycle hook).
- After you've shown me a pattern once, offer to let me write the next similar script myself, then review it for me.
- Remind me to **commit to Git before** starting a big change.

## REQUIRED: Editor setup instructions after every code change
Every time you finish writing or changing code, end your response with a section titled **"What to do in the Unity Editor"**. Be extremely detailed and specific, because I'm new to the editor. Include all of the following that apply:

1. **Wait for compile:** Tell me to switch back to Unity, wait for the loading spinner in the bottom-right corner to finish, and check the **Console** window (Window → General → Console) for red errors. If there are errors, tell me to paste them back to you.
2. **Creating objects:** Exact steps, e.g. "In the **Hierarchy** panel, right-click → **3D Object** → **Capsule**. Rename it to `Player` (right-click → Rename, or press F2)."
3. **Positioning:** Exact **Transform** values to type into the Inspector (Position X/Y/Z, Rotation, Scale).
4. **Attaching scripts and components:** e.g. "Select `Player` in the Hierarchy. In the **Inspector**, click **Add Component** at the bottom, type `PlayerMovement`, and press Enter." Also list any built-in components to add or remove (e.g. "Remove the Capsule Collider because CharacterController has its own").
5. **Inspector fields:** Every field I need to set, with the value to use. For object references, tell me exactly what to drag from where to where (e.g. "Drag `Player` from the Hierarchy into the **Target** slot on the `GuardChase` component").
6. **Tags and layers:** If code relies on a tag or layer, explain how to create it (Inspector → Tag dropdown → Add Tag → + button) and how to assign it.
7. **Package or project settings:** Exact menu paths for anything like baking the NavMesh, Input System actions, or Cinemachine cameras.
8. **Prefabs:** If something should be a prefab, explain how (drag it from the Hierarchy into `Assets/Prefabs/` in the Project window) and why.
9. **Save:** Remind me to save the scene (Ctrl+S / Cmd+S).
10. **Test it:** Tell me to press the **Play** button (top center), what controls to use, and **exactly what I should see if it worked**. Remind me that changes made during Play mode are lost when I stop playing.
11. **Troubleshooting:** List the 2–3 most likely things that go wrong for this step and how to fix each one (e.g. "falls through the floor → the plane is missing its collider").
12. **Commit:** Suggest a short Git commit message for this step.

Number the steps. Use the exact names of panels, menus, buttons, and fields as they appear in the Unity 6 editor.