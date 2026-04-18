# DevLog

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
