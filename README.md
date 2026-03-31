<div align="center">

# Metal Gear Mechanics

**Stealth-action gameplay systems inspired by Metal Gear Solid — built in Unity**

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-000000?logo=unity)
![C#](https://img.shields.io/badge/C%23-10-239120?logo=csharp)
![Cinemachine](https://img.shields.io/badge/camera-Cinemachine-blue)
![NavMesh](https://img.shields.io/badge/AI-NavMesh-orange)
![License](https://img.shields.io/badge/license-Educational-green)

[Features](#features) · [Controls](#controls) · [Getting Started](#getting-started) · [Project Structure](#project-structure)

</div>

---

## Overview

Metal Gear Mechanics is a third-person stealth-action game built in Unity that recreates core gameplay systems from the Metal Gear Solid series. Features include AI guard patrols with FOV-based detection, CQC grab mechanics, wall cover with dynamic camera transitions, a cardboard box disguise system, ranged combat with bullet spread and magazine management, and an inventory/loadout system.

<!-- Replace with a screenshot of the full game -->
![Game Overview](screenshots/header.png)

---

## Features

### Stealth & Detection System
AI guards patrol predefined waypoints with field-of-view detection. Sneak past enemies, use cover, and stay out of sight to avoid triggering alerts. When spotted, nearby guards are alerted and converge on your last known position.

<!-- Replace with a GIF showing stealth gameplay and getting detected -->
![Stealth Gameplay](screenshots/stealth.gif)

### Combat
Engage enemies with ranged weapons featuring realistic gunplay — bullet spread, magazine management, and muzzle flash VFX. Enemies return fire and search for the player when line-of-sight is broken.

<!-- Replace with a GIF showing combat and enemy reactions -->
![Combat Demo](screenshots/combat.gif)

### CQC Grab System
Get close to enemies and grab them from behind. Struggle mechanics determine whether you overpower the guard or they break free and alert others.

<!-- Replace with a GIF showing the grab, struggle, and kill/release -->
![CQC Grab Demo](screenshots/grab.gif)

### Cardboard Box
Hide in plain sight with the iconic cardboard box. Guards will ignore you while stationary, but moving while inside the box will blow your cover.

<!-- Replace with a GIF showing box stealth and getting caught while moving -->
![Cardboard Box Demo](screenshots/cardboard_box.gif)

### Wall Cover System
Stick to walls and peek around corners with a dynamic camera system that shifts perspective based on your position along the wall.

<!-- Replace with a GIF showing wall cover and camera transitions -->
![Wall Cover Demo](screenshots/wall_cover.gif)

### Inventory & Weapons
Pick up weapons and items throughout the level. Switch between weapons on the fly with a scrollable inventory UI.

<!-- Replace with a screenshot or GIF of the inventory system -->
![Inventory](screenshots/inventory.png)

---

## AI Behavior

Guards feature a full behavioral state machine:

| State | Behavior |
|-------|----------|
| **Patrol** | Follow waypoint routes with configurable wait times and look directions |
| **Caution** | Investigate suspicious activity with scan/search phases |
| **Aggressive** | Engage the player with ranged combat, reload cycles, and position tracking |
| **Search** | Sweep the area when the player breaks line-of-sight |

<!-- Replace with a GIF showing AI state transitions -->
![AI Patrol Demo](screenshots/ai_patrol.gif)

### Alert & Countdown System
When detected, an alarm countdown triggers across the HUD. Evade long enough and guards return to their patrol routes.

<!-- Replace with a screenshot of the alert HUD -->
![Alert System](screenshots/alert.png)

---

## Screenshots

| | |
|:---:|:---:|
| ![Screenshot 1](screenshots/screenshot_1.png) | ![Screenshot 2](screenshots/screenshot_2.png) |
| ![Screenshot 3](screenshots/screenshot_3.png) | ![Screenshot 4](screenshots/screenshot_4.png) |

---

## Controls

| Action | Input |
|--------|-------|
| Move | WASD |
| Aim | Right Mouse Button |
| Shoot | Left Mouse Button (while aiming) |
| Grab | Left Mouse Button (hold) |
| Crouch | C |
| Free Look | F |
| Switch Weapon | Q |
| Inventory | Left Ctrl |
| Pause / Menu | Escape |

---

## Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Engine** | Unity 2022.3 LTS | Game engine and runtime |
| **Language** | C# | All gameplay scripts |
| **Camera** | Cinemachine | Third-person, wall cover, and FPS cameras |
| **AI Navigation** | NavMesh | Pathfinding and agent movement |
| **Input** | Unity Input System + Legacy | Player controls |
| **UI** | TextMeshPro, Unity UI | HUD, inventory, menus |
| **Physics** | Unity Physics | Raycasting, triggers, collision detection |
| **VFX** | Particle System | Muzzle flash, hit effects |

---

## Getting Started

### Prerequisites

- Unity **2022.3 LTS** (any 2022.3.x patch)

### Run

```bash
git clone https://github.com/JakeeUp/MetalGearMechanics_Unity.git
```

1. Open the project in Unity Hub
2. Open `Assets/_MyAssets/Scenes/MainMenuScene`
3. Press Play

---

## Project Structure

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

---

## License

This project is for educational and portfolio purposes.
