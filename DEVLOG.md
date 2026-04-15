# DevLog

---

## [April 15, 2026] - Feature: Slide Kinematics & Animation Blending

**Features Implemented:**
*   Created `SlideState` and `CrouchState` utilizing the FSM architecture.
*   Added dynamic camera scaling to map the camera height to player "eye-line".
*   Bridged the movement state machine to the CitizenAnimationHelper component. The states are much easier to read when animated.

**Key Learnings & Takeaways:**
*   **Character Choice:** After testing the Deadlock characters, I've decided to model my movement values based on Yamato because they are one of the few characters fast enough to transition directly from running into sliding.
*   **De-coupling Visuals from Physics:** By decoupling the animations from the the physics state machine, I'm avoiding calculating graphics inside the physics system.

> 📷 **Media:**
[testing Yamato's movement velocities in Deadlock](https://outplayed.tv/media/7L2kzZ)

It seems like Yamato's slide has more directional control than my demo. I also think it takes longer to reach top speed on Yamato than in my demo. (It is difficult to tell so I will work on adding a state and velocity indicator to my demo)
![yamato slide control](https://media3.giphy.com/media/v1.Y2lkPTc5MGI3NjExcDkxOGdqZDc1bW5pc2dxbXFxcWVqOG91M2V5Mjhramh4bWJ3MWZiaiZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/5EKUZYYlLaNJPsV8ha/giphy.gif)
![my demo](https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExZmlqNWh4dHl6Y2xmczVybm51dmJwaTJ1ZnE2aGh6dzhud2hnODc4YSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/XQnm642yJ2ZNtE69D4/giphy.gif)

---

## [April 15, 2026] - Architecture Refactor: Finite State Machine

Before adding movement like sliding and dashing, I refactored the movement controller into a finite state machine to make the code easier to work with and more scalable.

**Architecture Updates:**
*   **The Manager Pattern:** Removed the physics maths from `movement.cs`. Now it acts as a state manager and holds global values for the movement states.
*   **State Encapsulation:** Made a `BaseState` and moved the grounded and airborne logic into `GroundedState.cs` and `AirborneState.cs`.
*   **Lifecycle Safety:** Implemented `Enter()` and `Exit()` methodology on the State transitions. For safety, only the manager can call these methods ( if i relied on the state to call its own Exit() method, I might forget to write it and cause a memory leak ).

---

## [April 14, 2026] - Feature: Recreating Deadlock's Momentum Physics

I implemented the core physics loop for the movement system, using manual physics instead of the engine's for more control.

**Features Implemented:**
*   **Manual Momentum Physics:**
*   **Jumping and Gravity:** Frame-rate independent Gravity and Jump impulses.
*   **Dynamic Friction:** Built a Source-engine style Ground Friction using Lerp.
*   **Air Strafing:** Implemented Dot Product acceleration mathematics.
*   **Camera Tracking:** Created a Custom Third-Person tracking script. (Still lacking many features of the Deadlock camera).

**Key Learnings & Takeaways:**
*   **The Dot Product Trick:** Instead of a hard speed limit, I used dot product to soft-clamp the acceleration. This way I can mimic the air strafe mechanics from Deadlock which allows you to gain speed past the clamp by using inputs perpendicular to your direction of motion.
*   **The Friction Tax:** I used Lerp for ground friction to smoothly deccelerate when letting go of any input which is probably used in Deadlock to punish landing while standing or failing a frame-perfect bunny hop. In Deadlock, there seems to be a much more aggressive friction when landing while moving faster than your ground speed ( I don't think it's due to the Lerp calculation; I think it's either different thresholds for friction values, or it isn't a linear scale ) - this is something I haven't implemented yet.
*   Learning that Deadlock uses the dot product for clamping speed made it much clearer to me why the movement works the way it does in-game.

> 📷 **Media:** ![manual ground and air movement physics](https://media4.giphy.com/media/v1.Y2lkPTc5MGI3NjExdXNyMGp5MXFpbGJldHpyODBtdnpta2VoYnV3YW9mb3NocnJndmJ3eSZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/F6lRRNaUyVG2K9I6Ck/giphy.gif)
