# Metal Gear Mechanics — Unity

A stealth-action game built in Unity inspired by the classic Metal Gear Solid series. Features third-person stealth gameplay with AI patrol systems, combat mechanics, inventory management, and cinematic camera transitions.

![Header Screenshot](screenshots/header.png)

---

## Features

### Stealth & Detection System
AI guards patrol predefined waypoints with field-of-view detection. Sneak past enemies, use cover, and stay out of sight to avoid triggering alerts. When spotted, nearby guards are alerted and converge on your last known position.

![Stealth Gameplay](screenshots/stealth.gif)

### Combat
Engage enemies with ranged weapons featuring realistic gunplay — bullet spread, magazine management, and muzzle flash VFX. Enemies return fire and search for the player when line-of-sight is broken.

![Combat](screenshots/combat.gif)

### CQC Grab System
Get close to enemies and grab them from behind. Struggle mechanics determine whether you overpower the guard or they break free and alert others.

![CQC Grab](screenshots/grab.gif)

### Cardboard Box
Hide in plain sight with the iconic cardboard box. Guards will ignore you while stationary, but moving while inside the box will blow your cover.

![Cardboard Box](screenshots/cardboard_box.gif)

### Wall Cover System
Stick to walls and peek around corners with a dynamic camera system that shifts perspective based on your position along the wall.

![Wall Cover](screenshots/wall_cover.gif)

### Inventory & Weapons
Pick up weapons and items throughout the level. Switch between weapons on the fly with a scrollable inventory UI.

![Inventory](screenshots/inventory.png)

### AI Behavior
Guards feature multiple behavioral states:
- **Patrol** — Follow waypoint routes with configurable wait times and look directions
- **Caution** — Investigate suspicious activity with scan/search phases
- **Aggressive** — Engage the player with ranged combat, reload cycles, and position tracking
- **Search** — Sweep the area when the player breaks line-of-sight

![AI Patrol](screenshots/ai_patrol.gif)

### Alert & Countdown System
When detected, an alarm countdown triggers across the HUD. Evade long enough and guards return to their patrol routes.

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

- **Engine:** Unity 2022.3 LTS
- **Language:** C#
- **Camera:** Cinemachine
- **Input:** Unity Input System + Legacy Input
- **AI Navigation:** NavMesh
- **UI:** TextMeshPro, Unity UI

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

## Getting Started

1. Clone the repository
2. Open in **Unity 2022.3** (LTS)
3. Open `Assets/_MyAssets/Scenes/MainMenuScene`
4. Press Play

---

## License

This project is for educational and portfolio purposes.
