# Platformer Player Setup

The prefab at `Assets/prefabs/PlatformerController.prefab` is set up as a proper player prefab:

- `Player` root GameObject
  - `CharacterController`
  - `PlatformerPlayerController`
- `Body` child GameObject
  - `SkinnedModelRenderer` using `models/citizen/citizen.vmdl`
  - `CitizenAnimationHelper` targeting the skinned model renderer

The startup scene (`Assets/scenes/minimal.scene`) includes a Player instance at `0,0,80`. The scene camera is assigned directly to the player's `PlatformerPlayerController.CameraTarget`, so movement, camera follow, respawn, and animation are handled by one component.

## Controls

Movement uses `Input.AnalogMove.x` first, so keyboard movement and gamepad left-stick movement work with s&box defaults. Named-action fallback fields are now exposed on the controller:

- `LeftAction` defaults to `Left`
- `RightAction` defaults to `Right`
- `JumpAction` defaults to `Jump`
- `RunAction` defaults to `Run`
- `DuckAction` defaults to `Duck`

If one of those names does not exist in Project Settings/Input, either add the input action or change the string on `PlatformerPlayerController`.

## Citizen animations

The controller drives the Citizen model's built-in animation graph through `CitizenAnimationHelper`.

It feeds these values every frame:

- actual controller velocity via `WithVelocity`
- requested/wish movement via `WithWishVelocity`
- grounded state via `IsGrounded`
- walk/run state via `MoveStyle`
- facing/look direction via `WithLook`
- jump starts via `TriggerJump`
- optional duck/crouch blend via `DuckLevel`

This means the Citizen skinned model uses its own idle, walk, run, jump, fall, landing, and crouch-style graph behavior instead of the controller forcing raw sequence names.

Important prefab references:

- `PlatformerPlayerController.BodyRenderer` should point to the `Body` skinned model renderer.
- `PlatformerPlayerController.AnimationHelper` should point to the `Body` citizen animation helper.
- `CitizenAnimationHelper.Target` should point to the `Body` skinned model renderer.
- `SkinnedModelRenderer.UseAnimGraph` should stay enabled.

## Current controller features

- Walk and run movement.
- Ground/air acceleration and ground friction.
- Faster turn-around acceleration for responsive direction changes.
- Input dead zone for stick drift.
- Configurable input action names.
- Coyote time and jump buffering.
- Variable jump height.
- Apex-hang gravity for less floaty but more readable jumps.
- Faster falling, capped max fall speed, and ground-stick velocity.
- Optional extra air jumps.
- 2.5D Y-axis lock.
- Facing direction on the visual child only, so the root/controller stays clean.
- Citizen animation helper integration including jump and duck values.
- Built-in side-view camera follow with a player focus offset, look-ahead, smoothed look-ahead, small dead zone, snap-on-start, and automatic look-at rotation so the player stays framed.
- Fall respawn using a configurable `KillZ`, `RespawnPoint`, and `SafeGroundRespawnHeight`.

## Camera setup

The controller now treats the camera as a side-view platformer camera:

- `CameraOffset` defaults to `0,-620,140`, which keeps the camera behind the 2.5D plane instead of offsetting heavily on X.
- `CameraFocusOffset` defaults to `0,0,48`, so the camera looks at the Citizen body/chest area instead of the player's feet.
- `CameraLooksAtPlayer` keeps the camera rotated toward that focus point every frame.
- `CameraLookAhead` is intentionally modest so the player remains visible while moving.

If the player is too high/low in frame, tune `CameraFocusOffset.z` first. If the camera is too close/far, tune `CameraOffset.y`.

## Useful tuning values

Most values are exposed as `[Property]` fields on `PlatformerPlayerController`:

- `WalkSpeed` / `RunSpeed`
- `GroundAcceleration` / `AirAcceleration`
- `TurnAroundMultiplier`
- `GroundFriction`
- `InputDeadZone`
- `JumpSpeed`
- `Gravity`
- `FallGravityMultiplier`
- `LowJumpGravityMultiplier`
- `ApexHangGravityMultiplier`
- `ApexHangVelocity`
- `MaxFallSpeed`
- `GroundStickVelocity`
- `CoyoteTime`
- `JumpBufferTime`
- `ExtraAirJumps`
- `CameraTarget`
- `CameraOffset`
- `CameraFocusOffset`
- `CameraLooksAtPlayer`
- `CameraFollowSharpness`
- `CameraLookAhead`
- `CameraLookAheadSharpness`
- `CameraVerticalLookAhead`
- `CameraDeadZoneX` / `CameraDeadZoneZ`
- `BodyRenderer`
- `AnimationHelper`
- `UseCitizenAnimation`
- `TriggerCitizenJumpAnimation`
- `AnimationSpeedDeadZone`
- `AnimationDuckLerpSpeed`
- `RespawnWhenFalling`
- `KillZ`
- `RespawnPoint`
- `SafeGroundRespawnHeight`

## Notes for extending

`PlatformerPlayerController` exposes `IsGrounded`, `Velocity`, `MoveInput`, `Facing`, `IsRunActive`, `IsJumpHeld`, `IsDuckHeld`, and `CanBufferedJump`, which makes it easier to add sound effects, dust particles, attack states, health, checkpoints, or state logic without rewriting the movement core.

The controller locks the player to the Y depth axis for a 2.5D platformer. Disable `LockDepthAxis` if you want free 3D movement.


## Jump movement fix

The controller now launches jumps through `CharacterController.Punch( Vector3.Up * JumpSpeed )` by default.

This is intentional: directly setting `CharacterController.Velocity.z` while the controller still thinks it is grounded can trigger the Citizen jump animation without physically separating the controller from the floor. `Punch` is the safer s&box CharacterController path for jump impulses.

New jump-related tuning fields:

- `UseCharacterControllerPunchForJump` - keep this enabled for normal jumping.
- `JumpUngroundNudge` - tiny upward nudge after jumping to avoid same-tick ground snapping.
- `PostJumpGroundGraceTime` - brief grace window where animation/logic treats the player as airborne immediately after a jump.
