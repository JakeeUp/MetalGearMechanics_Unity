<div align="center">

# Metal Gear Mechanics

A third-person stealth game in Unity, built around mechanics from Metal Gear Solid

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000000?logo=unity)
![C#](https://img.shields.io/badge/C%23-10-239120?logo=csharp)
![Cinemachine](https://img.shields.io/badge/camera-Cinemachine-blue)
![NavMesh](https://img.shields.io/badge/AI-NavMesh-orange)
![License](https://img.shields.io/badge/license-Educational-green)

[Features](#features) · [Controls](#controls) · [Getting Started](#getting-started) · [Project Structure](#project-structure)

</div>

<p align="center"><img src="screenshots/header.gif" alt="Bridge gunfight"></p>

## Overview

This is my attempt at recreating the core systems from Metal Gear Solid in Unity. Guards patrol and spot you with a field-of-view check, you can grab them from behind with CQC, hug walls while the camera swings around, hide under a cardboard box, and shoot your way out when all of that fails. Guns have bullet spread and magazines, and there's an inventory for the weapons and items you pick up.

## Features

### Stealth and detection

Guards walk set waypoint routes and check for you inside a view cone. If one spots you, the guards nearby get your last known position and head there, so the goal is to stay out of sight and use cover.

<p align="center"><img src="screenshots/stealth.gif" alt="Stealth gameplay"></p>

### Combat

Shots have spread, you have to manage your magazine, and the muzzle flash is a particle effect. Guards shoot back, and if you break line of sight they go looking for you.

<p align="center"><img src="screenshots/combat.gif" alt="Combat demo"></p>

### CQC grab

Sneak up behind a guard and grab them. They'll struggle, and either you overpower them or they break free and alert the others.

<p align="center"><img src="screenshots/grab.gif" alt="CQC grab demo"></p>

### Cardboard box

Guards ignore the box as long as it stays still. Move while they're looking and you're caught.

<p align="center"><img src="screenshots/cardboard_box.gif" alt="Cardboard box demo"></p>

### Wall cover

Press against a wall to lean and peek around corners. The camera changes angle depending on where you are along the wall.

<p align="center"><img src="screenshots/wall_cover.gif" alt="Wall cover demo"></p>

### Inventory and weapons

You pick up weapons and items around the level and swap between them from a scrollable inventory.

<p align="center"><img src="screenshots/inventory.png" alt="Inventory"></p>

## AI behavior

Each guard runs a state machine with four states:

| State | What the guard does |
|-------|----------|
| Patrol | Walks a waypoint route, with a wait time and look direction you can set per point |
| Caution | Investigates something suspicious by scanning and searching |
| Aggressive | Shoots at the player, reloads, and keeps track of where the player is |
| Search | Sweeps the area after losing line of sight |

<p align="center"><img src="screenshots/ai_patrol.gif" alt="AI patrol demo"></p>

### Alert countdown

Getting spotted starts an alarm countdown on the HUD. If you stay hidden until it runs out, the guards go back to their patrols.

<p align="center"><img src="screenshots/alert.png" alt="Alert HUD"></p>

## Screenshots

| | |
|:---:|:---:|
| ![Screenshot 1](screenshots/screenshot_1.png) | ![Screenshot 2](screenshots/screenshot_2.png) |
| ![Screenshot 3](screenshots/screenshot_3.png) | ![Screenshot 4](screenshots/screenshot_4.png) |

## Controls

| Action | Input |
|--------|-------|
| Move | WASD |
| Aim | Right mouse button |
| Shoot | Left mouse button (while aiming) |
| Grab | Hold left mouse button |
| Crouch | C |
| Free look | F |
| Switch weapon | Q |
| Inventory | Left Ctrl |
| Pause / menu | Escape |

## Tech stack

| Layer | Technology | Used for |
|-------|-----------|---------|
| Engine | Unity 2022.3 LTS | Game engine and runtime |
| Language | C# | All gameplay scripts |
| Camera | Cinemachine | Third-person, wall cover, and FPS cameras |
| AI navigation | NavMesh | Pathfinding and agent movement |
| Input | Unity Input System + legacy input | Player controls |
| UI | TextMeshPro, Unity UI | HUD, inventory, menus |
| Physics | Unity Physics | Raycasts, triggers, collisions |
| VFX | Particle System | Muzzle flash and hit effects |

## Getting started

You need Unity 2022.3 LTS (any 2022.3.x patch works).

```bash
git clone https://github.com/JakeeUp/MetalGearMechanics_Unity.git
```

1. Open the project in Unity Hub
2. Open `Assets/_MyAssets/Scenes/MainMenuScene`
3. Press Play

## Project structure

```
Assets/_MyAssets/
├── Scripts/
│   ├── AI/                 # Enemy AI controller, patrol, detection, combat
│   ├── Controller/         # Player controller, input handling
│   ├── Items/              # Weapons, pickups, cardboard box, consumables
│   ├── Managers/           # Inventory, camera, resources, game management
│   ├── SceneLoading/       # Level transitions, scene management
│   ├── UI/                 # HUD, inventory UI, countdown display
│   └── Utilities/          # Object pooling, interfaces, helpers
├── Prefabs/
├── Scenes/
├── Audio/
└── Art/
```

## License

This project is for educational and portfolio use.
