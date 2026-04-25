# Deadlock Movement System
> This project aims to replace the standard Facepunch PlayerController with a from-scratch C# implementation to mimic the momentum-based mechanics found in *Deadlock*.

## Current Status: Implementing Core Mechanics
- [x] CharacterController Integration
- [x] Camera-relative WishDir Projection
- [x] Air Strafe Physics
- [x] Dash Mechanics
- [x] Double-Jumping
- [x] Dash-Jump
- [x] Wall Bouncing
- [x] Sliding on slopes (min. slide-speed variation)
- [x] Edge Boosting
- [ ] Mantling
- [ ] (Bonus) Mantle Sliding
- [ ] (Environmental) Ziplines
- [ ] (Environmental) Vents
- [ ] Stamina System



## Technical Goal
> To implement a momentum-conservative state machine that handles stamina-gated movement actions while maintaining the classic Source "snappiness" through custom WishDir projection.

## Calibration Backlog
> Physics values to measure and match against the Deadlock reference (+ other small features/tweaks).
- [ ] input-based directional impulse on double jumps

