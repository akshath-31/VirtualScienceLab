# Virtual Science Lab

A desktop-based interactive 3D Virtual Science Laboratory developed using **Unity 6.5** and **C#**. The project provides a first-person laboratory environment where users can explore scientific equipment, interact with objects, view educational information and perform basic virtual experiments.

## Project Overview

Virtual Science Lab demonstrates how a traditional science laboratory can be recreated as an interactive virtual environment.

Users can navigate through a 3D laboratory, inspect scientific equipment and perform interactions such as picking up beakers and transferring liquid between them.

The application runs on a standard desktop PC using keyboard and mouse controls. A VR headset is not required.

## Features

### 1. 3D Laboratory Environment
- Fully modelled virtual science laboratory.
- Workbenches, storage cabinets, shelves and laboratory equipment.
- Microscope, beakers, test tubes and computer workstation.
- Real-time lighting and material design.

### 2. First-Person Navigation
- WASD-based player movement.
- Mouse-controlled camera rotation.
- Jumping and gravity.
- Collision detection with laboratory objects and walls.

### 3. Interactive Equipment Inspection
- Camera-based object detection using Raycasting.
- Contextual interaction prompts.
- Interactive information panels.
- Educational descriptions of laboratory equipment.

### 4. Beaker Pickup and Liquid Pouring
- Pick up and drop beakers.
- Hold and move beakers while navigating.
- Tilt beakers to pour liquid.
- Visual liquid stream between beakers.
- Dynamic source and destination liquid levels.
- Liquid capacity management and overflow prevention.

### 5. Main Menu and User Interface
- Minimalist main menu.
- Start Simulation option.
- How to Play instructions.
- Exit functionality.
- Contextual interaction hints.

## Technologies Used

| Technology | Purpose |
|---|---|
| Unity 6.5 | 3D development platform |
| C# | Gameplay scripting |
| Universal Render Pipeline (URP) | Rendering |
| Unity New Input System | Keyboard and mouse input |
| CharacterController | First-person movement |
| Raycasting | Equipment detection |
| Unity UI / Canvas | Menus and information panels |
| LineRenderer | Visual liquid-pouring stream |
| Unity AI Assistant | Development assistance |

## Controls

| Key | Action |
|---|---|
| W / A / S / D | Move |
| Mouse | Look around |
| Space | Jump |
| E | Inspect equipment |
| F | Pick up / Drop beaker |
| Hold R | Pour liquid |
| Escape | Release mouse cursor |
| Left Click | Lock mouse cursor again |

## Interactive Equipment

The laboratory includes:
- Compound Light Microscope
- Beakers with coloured liquid
- Test tubes
- Computer workstation
- Laboratory storage equipment

Users can inspect supported equipment to view its purpose and basic usage information.

## Installation and Setup

### Requirements
- Windows PC
- Unity Hub
- Unity Editor 6.5 (6000.5.8f1)
- Keyboard and mouse

### Steps to Run

1. Clone or download this repository.
2. Open Unity Hub.
3. Select **Add Project from Disk**.
4. Navigate to the downloaded `VirtualScienceLab` folder.
5. Open the project using Unity 6.5.
6. Open the `MainMenu` scene.
7. Press Play to launch the simulation.

If the MainMenu scene is not automatically available, locate it under the project's `Assets/Scenes` directory.

## Project Structure

```text
VirtualScienceLab/
│
├── Assets/
│   ├── Scenes/
│   │   ├── MainMenu
│   │   └── SampleScene
│   │
│   └── VirtualScienceLab/
│       └── Scripts/
│           ├── FirstPersonController.cs
│           ├── PlayerInteraction.cs
│           ├── InteractableObject.cs
│           ├── LiquidContainer.cs
│           ├── PickupBeaker.cs
│           └── PlayerBeakerController.cs
│
├── Packages/
├── ProjectSettings/
├── .gitignore
└── README.md
```

## Implementation Highlights

- Built the laboratory environment using Unity 3D objects and materials.
- Implemented first-person navigation using CharacterController.
- Developed object interaction using camera-based Raycasting.
- Created reusable liquid container components with independent fill levels.
- Implemented visual liquid transfer using animated beaker rotation and LineRenderer.
- Developed a separate main menu and integrated scene navigation.

## Limitations

- This is a desktop-based virtual laboratory simulation, not a headset-based VR application.
- Liquid transfer is visually simulated rather than based on real fluid dynamics.
- Laboratory equipment is intended for educational demonstration.
- Advanced chemical reactions, spill physics and glass breakage are not implemented.

## Project Objective

To demonstrate the development of an interactive virtual learning environment that enables students to explore laboratory equipment and understand basic laboratory operations through a desktop-based 3D simulation.

---

**Developed as an academic project using Unity 6.5.**
