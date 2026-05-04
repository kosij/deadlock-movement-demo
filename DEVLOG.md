# DevLog

## 11 [May 2, 2026] - Feature: Stamina System & UI

<br><br>

**Features Implemented:**
*   **StaminaComponent:** A standalone component with a `TryConsume(float cost)` API and time-based regen (`0.25` charges/s, ~4s per charge). Separated from `Movement.cs` so the HUD and future ability systems can reference it without coupling to the movement stack.
*   **Stamina-Gated Actions:** All movement actions now require available charges: dashes cost `1.0`, double jumps cost `1.0`, the dash jump costs an additional `1.0` on top of the dash (2.0 total), and non-first wall jumps cost `0.5`. The first wall jump remains free. Per-airtime flags (`HasAirDashed`, `HasDoubleJumped`) are unchanged - stamina is a second, independent gate.
*   **Stamina UI & Crosshair:** Added a centered crosshair dot with 3 stamina pips below it. Pips drain as charges are spent. Pips flash red for `0.4s` when a gated action fails. Pips flash blue during the valid dash-jump window using a pattern match on `CurrentState as DashState`.

<br><br>

**Key Learnings & Takeaways:**
*   **Stamina as a Design Tool:** The reason Deadlock uses a stamina system isn't to limit fun - it's to force decisiveness. Every action has a cost and you have to commit to what you're spending charges on.
*   **Component Separation:** Stamina as a standalone component means any future system - abilities, items, UI - can reference it directly without coupling to the movement stack. `TryConsume()` is a clean, reusable gate, and the HUD references the component directly without going through the movement manager.

<br><br>

> **Media:**



---

## 10 [April 29, 2026] - Feature: Mantling & Mantle Slide

<br><br>

**Features Implemented:**
*   **Two-Phase Ledge Detection:** Implemented `TryGetMantleTarget()` using a 5-ray forward sweep to find the nearest wall face, then a downward scan from above the wall to find the true top surface. This handles thin rails, high ledges, and fences that a standard chest/head height raycast would miss.
*   **Arced MantleState:** The climb uses independent sine curves for Z (EaseOutSine - pulls up fast) and XY (EaseInSine - pushes forward late) rather than a single linear lerp. This creates a "pull up, push over" arc that prevents the player capsule from clipping through the wall corner.
*   **Explicit Mantle Slide:** Testing Deadlock revealed the mantle slide injects exactly `512 u/s` on exit rather than carrying whatever velocity the player had into the wall. The standard exit is `177 u/s`. Triggered by holding crouch and directing input toward the wall on mantle completion.
*   **Auto-Mantle from Ground:** Implemented via `Input.Down("jump") && !Input.Pressed("jump")` - if jump is held from a previous action and the player walks into a valid obstacle, they auto-mantle without a fresh jump press.

<br><br>

**Key Learnings & Takeaways:**
*   **Two-Phase Detection vs Chest/Head Raycasts:** The common approach fires rays at chest height and head height to detect if an obstacle can be vaulted. The two-phase method (find wall face, then find its top) is less common but handles a wider range of geometry - particularly thin obstacles like rails and ledges where the head ray would pass clean through.
*   **Mantle Slide is Coded, Not Emergent:** I initially assumed the mantle slide was emergent behaviour - the slide state picking up whatever exit velocity the mantle carried, but testing Deadlock proved otherwise. The `512 u/s` value appears consistently regardless of entry speed, which means it's an explicit injection at the transition point, not physics preserving momentum.

<br><br>

> **Media:**



---

## 9 [April 25, 2026] - Feature: Edge Boosting & Telemetry HUD

<br><br>

**Features Implemented:**
*   **Edge Boosting (Wall Coyote Time):** Implemented a `TimeSinceLeftWall` timer that keeps the wall jump window open for `0.15s` after leaving a wall. This makes edge boosts ( sliding along a wall past its corner edge before wall jumping and getting a diagonal kick ) consistently accessible rather than frame-perfect.
*   **Directional Impulse on Double Jump:** Added a `WallJumpInputBoost` burst to the double jump, matching Deadlock's behaviour where holding a direction before an air jump steers your trajectory.
*   **Telemetry HUD:** Added a debug panel showing coyote window status, timer value, and `HasWallJumped` flag. Used to validate and diagnose wall jump behaviour in real-time.
*   **Dash from WallSlideState:** Added a missing transition that previously made dash inaccessible while wall sliding.
*   **Camera Collision:** Added a sphere-cast in `PlayerCamera.cs` to prevent the camera from clipping through walls.

<br><br>

**Key Learnings & Takeaways:**
*   **Telemetry-Driven Debugging:** The coyote wall jump felt slightly wrong but was hard to pinpoint without data. Building the HUD immediately caught the root cause - `LastWallNormal` was a zero vector on the exit frame, silently degrading the wall jump to a double jump.
*   **Live Raycasts over Stored State:** After fixing the zero vector bug, the HUD's `Last Wall N:` readout revealed a second problem - the stored normal was always a pure value (e.g., `1.0, 0.0`) at corners, never the diagonal needed for an edge boost. Switched to firing a live 64-unit starburst at the moment of the coyote jump instead. When mid-corner, rays hit both Wall 1 and Wall 2 and the averaged normal is correctly diagonal, producing the forward boost.
*   **VFX as validation:** Without directional feedback on the kick (no bounce VFX or vector indicator), evaluating the edge boost against the Deadlock reference is difficult. The trajectory feels intentional and natural, but a proper 1:1 comparison might need visual VFX on the kick direction. I am planning to implement a colour-coded vector overlay system to make trajectories and interactions clearer.

<br><br>

> **Media:**



---

## 8 [April 23, 2026] - Feature: Dynamic Slope Sliding

<br><br>

**Features Implemented:**
*   **Dynamic Slope-Sliding Threshold:** Implemented a system that reduces the `MinSlideSpeed` requirement to 0 if the player's horizontal velocity is moving downhill. This allows the player to transition into a slide almost instantly when moving in the downhill direction of a slope.
*   **Gravity-Assisted Sliding:** Updated `SlideState` to inject artificial gravity acceleration into the player's horizontal momentum when sliding downhill. This ensures the player maintains or gains speed smoothly when coasting down stairs without holding movement keys.

<br><br>

**Key Learnings & Takeaways:**
*   **Zero-Friction Gravity Injection:** My movement logic zeroes out the Z-axis velocity to keep the player snapped to the floor so sliding downhill doesn't naturally accelerate the player which feels unnatural. Manually applying a gravity multiplier to the horizontal velocity based on the slope gradient makes the slide feel much better.

<br><br>

> **Media:**



<br><br>

## 7 [April 22, 2026] - Feature: Test Arena & Wall Jumping

<br><br>

**Features Implemented:**
*   **Test Arena Geometry:** Built a greybox testing environment containing a slanted plane, and vertical walls to serve as the physical testing ground for upcoming mechanics.
*   **Wall Bouncing & WallSlideState:** Implemented Deadlock's Wall Jump physics. Added an 8-way directional raycast to `AirborneState` to detect walls. Colliding with a wall transitions the player into a new `WallSlideState`. Jumping off the wall averages the normals of all walls being touched, completely wipes velocity along that normal, preserves parallel velocity, and applies a flat outward kick and upward pop. Consecutive wall jumps don't provide any pop, only the kick. The player's directional input also applies a direct impulse burst to manipulate the bounce trajectory.

<br><br>

**Key Learnings & Takeaways:**
*   **Slope-Sliding Physics Analysis:** I discovered a mismatch in slide behavior when interacting with slopes. In Deadlock, sliding down a slope requires almost no initial momentum, while sliding upwards requires the standard threshold. This is probably calculated using the Dot Product between the player's velocity and the ground normal (to determine if their momentum is carrying them downhill), and then applies a multiplier to the `MinSlideSpeed` requirement. I logged this in the backlog as a feature to implement.
*   **Wall Detection Methods:** Initially, I tried to detect walls by checking if the player's collision box was overlapping a wall, but the physics engine couldn't tell which side of the wall was hit, so it always bounced the player in the exact same default direction. I fixed this by shooting 8 raycasts outwards from the center of the player. When a raycast hits the wall we get the exact normal of the surface hit.

<br><br>

> **Media:**



<br><br>

---

## 6 [April 19, 2026] - Feature: Double Jump & Dash-Jump

<br><br>

**Features Implemented:**
*   **Double Jump:** Added a second jump available while airborne using a `HasDoubleJumped` flag which resets on landing. The `AirJumpForce` value is a rough estimate for now and will need to be measured against the Deadlock reference before it can be considered calibrated.
*   **Dash-Jump:** A skill-gated jump available only in the tail end of a ground dash. Pressing jump too early locks out the window for that dash. The most interesting part was reverse-engineering the trajectory from two observations in Deadlock: the total 3D velocity (`806`) and a visual estimate that the jump height was roughly double a regular jump. With gravity confirmed as `800` in both systems, I was able to work backwards to solve for the required horizontal and vertical components and reproduce the same trajectory.

<br><br>

**Key Learnings & Takeaways:**
*   **`Controller.Punch()` vs direct velocity assignment:** The regular jump and dash-jump both use `Punch()` to apply the upward impulse because the player is grounded ( I might need to review this when I add things like coyote time or input buffering ). `Punch()` is processed after the controller's ground-snapping pass inside `Move()`, so it is guaranteed to apply the impulse. The double jump uses direct `targetVelocity.z` assignment instead, because the player is already airborne and there is no ground-snap to fight. Getting this wrong caused the dash-jump to log as successful but produce no visible jump until the distinction was made.

<br><br>

> **Media:**

https://github.com/user-attachments/assets/db0298f9-545e-4dbe-911a-47f1eaed2f47

<br><br>

---

## 5 [April 17, 2026] - Feature: Dash State & Soft-Cap Air Physics


<br><br>

**Features Implemented:**
*   **Dash State:**  Implemented a time-locked DashState with distinct speeds for ground (579) and air (527). The physics velocity is there and matches Deadlock's values, but the overall feel is smoother and lacks the punchiness of the reference. I think the native interpolation of the CitizenAnimationHelper is softening the impact, and missing VFX and camera movement probably account for the rest.
*   **Slide Queuing from the Air:**  When landing from a dash while holding crouch, the player drops directly into a slide if above the speed threshold. I also noticed when testing in Deadlock that crouching from airborne seems to briefly flash a standing frame first ( it could just be that the falling pose and standing pose are similar ) - which suggests there might not be a direct airborne → crouch transition in the reference. I haven't confirmed this but it informed how I wired the landing logic.
*   **Soft-Cap Air Drag:** Added a Lerp-based drag to `AirborneState` that bleeds lateral speed down to 300 units.
*   **Prototype Slide Animation:** Bypassed the `CitizenAnimationHelper`'s walk-cycle blending by passing `Vector3.Zero` to `Animator.WithVelocity()` during slides and dashes, which freezes the legs in a static pose. It doesn't look great, but it makes it much easier to visually distinguish between movement states while testing the physics.
*   **Dash Interrupt Behaviour:**  Pressing crouch during a dash will interrupt it and drop the player into a slide. Holding crouch since before the dash began will not interrupt it, instead the player exits the dash naturally and lands in a crouch. This is implemented using `Input.Pressed` for the interrupt check and `Input.Down` for the post-dash landing check.

<br><br>

**Key Learnings & Takeaways:**
*   **Air Drag Investigation:** A normal running jump was consistently landing above the slide speed threshold, which didn't match Deadlock's behaviour. I wasn't sure if air drag even existed in the reference, so the mismatch prompted me to test further. I used Mina to test this because she has a glide ability to roughly neutralize her Z velocity while airborne. This made it easier to isolate whether lateral speed was actually decaying through the air. It was, so I implemented a Lerp-based soft cap to match.

<br><br>

> **Media:**

`testing presence of air drag using Mina`

https://github.com/user-attachments/assets/442a153b-e1cc-461a-9692-75e02c74204b

while limiting the z velocity as much as possible, we can observe a clear velocity decay

<br><br>

`air dash -> slide`

https://github.com/user-attachments/assets/beeeadbd-526e-4ed0-ad85-722ef449a242

<br><br>

`ground dash -> slide interrupt`

https://github.com/user-attachments/assets/33e24972-dbae-46ec-a4c6-5da9be5e2b60

pressing crouch during a ground dash will interrupt with a slide

<br><br>

`crouch -> ground dash`

https://github.com/user-attachments/assets/664d31b4-8871-4e5e-9438-feeb837a0f24

holding crouch since before the dash results in the full uninterrupted dash duration


<br><br>

---

<br><br>

## 4 [April 16, 2026] - Feature: Telemetry HUD & Slide Kinematics

<br><br>

**Features Implemented:**
*   **Real-time Telemetry HUD:** Built a custom UI overlay using Razor to track absolute velocity and xyz velocities relative to the camera. Added a rolling 2-second peak velocity measurement to read the speed limits easier.
*   **Slide Steering:** I added the same style acceleration and speed mechanics to the slide as other states to allow for steering while sliding

<br><br>

**Key Learnings & Takeaways:**
*   **Vector Normalisation:** When I stripped the z-axis from my input vector (`wishDir.z = 0`), I was shrinking the vector's absolute length whenever the camera was pitched up or down or when moving diagonally. I used `wishDir = wishDir.Normal` to normalise my wish vector and make my movement the same in every direction.
*   **Hidden Dependency Error:** I ran into an issue where the s&box Editor refused to compile when I removed the native 'Player Controller' component because the player prefab I was using seemed to have a hidden dependency on it. I first attempted to fix the problem by clearing the engine cache, but finally resolved the issue by migrating all my custom components to an empty game object.

<br><br>

> **Media:**

`diagonal movement is slower`

https://github.com/user-attachments/assets/a03fff6f-7dd2-41bd-bfd9-cebbbd3ece62

<br><br>

`fixed diagonal movement`

https://github.com/user-attachments/assets/28d86471-21b3-410a-abbc-273f7f04487b

<br><br>

`can now steer while sliding`

https://github.com/user-attachments/assets/b676b8f7-fa30-40f0-8934-37129f715c6d

The hud lets us know we are sliding, but I need to add a slide animation so I can get a better feel of the physics.

<br><br>

`you can slide infinitely with a precise run radius`

https://github.com/user-attachments/assets/eff8b332-e15c-4f35-b759-0b97d4da9449

This is an unintended mechanic. I will need to tweak some physics values or clamp the turning speed another way.

<br><br>

---

<br><br>

## 3 [April 15, 2026] - Feature: Slide Kinematics & Animation Blending

<br><br>

**Features Implemented:**
*   Created `SlideState` and `CrouchState` utilizing the FSM architecture.
*   Added dynamic camera scaling to map the camera height to player "eye-line".
*   Bridged the movement state machine to the CitizenAnimationHelper component. The states are much easier to read when animated.

<br><br>

**Key Learnings & Takeaways:**
*   **Character Choice:** After testing the Deadlock characters, I've decided to model my movement values based on Yamato because they are one of the few characters fast enough to transition directly from running into sliding.
*   **De-coupling Visuals from Physics:** By decoupling the animations from the the physics state machine, I'm avoiding calculating graphics inside the physics system.

<br><br>

> **Media:**

`testing Yamato's movement velocities in Deadlock`

https://github.com/user-attachments/assets/41eec0da-ea96-4672-8880-3a48ad2df821

After testing, I figured out the minimum speed required to slide is ~350 units of velocity

<br><br>

`yamato's slide control`

https://github.com/user-attachments/assets/6dfa4232-a540-4dbe-a9bc-33daccef1615

It seems like Yamato's slide has more directional control than my demo. I also think it takes longer to reach top speed on Yamato than in my demo. (It is difficult to tell so I will work on adding a state and velocity indicator to my demo).

<br><br>

`my demo (I don't have a slide animation yet)`

<img width="426" height="240" alt="Video Project" src="https://github.com/user-attachments/assets/43eca47e-df84-408a-8f35-deee4669c296" />

<br><br>

---

<br><br>

## 2 [April 15, 2026] - Architecture Refactor: Finite State Machine

<br><br>

Before adding movement like sliding and dashing, I refactored the movement controller into a finite state machine to make the code easier to work with and more scalable.

**Architecture Updates:**
*   **The Manager Pattern:** Removed the physics maths from `movement.cs`. Now it acts as a state manager and holds global values for the movement states.
*   **State Encapsulation:** Made a `BaseState` and moved the grounded and airborne logic into `GroundedState.cs` and `AirborneState.cs`.
*   **Lifecycle Safety:** Implemented `Enter()` and `Exit()` methodology on the State transitions. For safety, only the manager can call these methods ( if i relied on the state to call its own Exit() method, I might forget to write it and cause a memory leak ).

<br><br>

---

<br><br>

## 1 [April 14, 2026] - Feature: Recreating Deadlock's Momentum Physics

<br><br>

I implemented the core physics loop for the movement system, using manual physics instead of the engine's for more control.

**Features Implemented:**
*   **Manual Momentum Physics:**
*   **Jumping and Gravity:** Frame-rate independent Gravity and Jump impulses.
*   **Dynamic Friction:** Built a Source-engine style Ground Friction using Lerp.
*   **Air Strafing:** Implemented Dot Product acceleration mathematics.
*   **Camera Tracking:** Created a Custom Third-Person tracking script. (Still lacking many features of the Deadlock camera).

<br><br>

**Key Learnings & Takeaways:**
*   **The Dot Product Trick:** Instead of a hard speed limit, I used dot product to soft-clamp the acceleration. This way I can mimic the air strafe mechanics from Deadlock which allows you to gain speed past the clamp by using inputs perpendicular to your direction of motion.
*   **The Friction Tax:** I used Lerp for ground friction to smoothly deccelerate when letting go of any input which is probably used in Deadlock to punish landing while standing or failing a frame-perfect bunny hop. In Deadlock, there seems to be a much more aggressive friction when landing while moving faster than your ground speed ( I don't think it's due to the Lerp calculation; I think it's either different thresholds for friction values, or it isn't a linear scale ) - this is something I haven't implemented yet.
*   Learning that Deadlock uses the dot product for clamping speed made it much clearer to me why the movement works the way it does in-game.

<br><br>

> **Media:**

`manual ground and air movement physics`

<img width="480" height="270" alt="gif" src="https://github.com/user-attachments/assets/072fed97-96c6-4a76-a3c1-44db6b1b5a79" />

<br><br>
