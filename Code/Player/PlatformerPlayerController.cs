using System;
using Sandbox.Citizen;
using Sandbox.UI;

public sealed class PlatformerPlayerController : Component
{
	[Property, RequireComponent]
	public CharacterController Controller { get; set; }

	[Header( "Input" )]
	[Property] public string LeftAction { get; set; } = "Left";
	[Property] public string RightAction { get; set; } = "Right";
	[Property] public string JumpAction { get; set; } = "Jump";
	[Property] public string RunAction { get; set; } = "Run";
	[Property] public string DuckAction { get; set; } = "Duck";
	[Property] public string AttackAction { get; set; } = "Attack1";
	[Property] public bool UseAnalogMoveInput { get; set; } = false;

	[Header( "Movement" )]
	[Property] public float WalkSpeed { get; set; } = 285.0f;
	[Property] public float RunSpeed { get; set; } = 425.0f;
	[Property] public float GroundAcceleration { get; set; } = 22.0f;
	[Property] public float AirAcceleration { get; set; } = 10.0f;
	[Property] public float TurnAroundMultiplier { get; set; } = 1.45f;
	[Property] public float GroundFriction { get; set; } = 14.0f;
	[Property] public float InputDeadZone { get; set; } = 0.12f;

	[Header( "Jumping" )]
	[Property] public float JumpSpeed { get; set; } = 405.0f;
	[Property] public float Gravity { get; set; } = 1020.0f;
	[Property] public float FallGravityMultiplier { get; set; } = 1.65f;
	[Property] public float LowJumpGravityMultiplier { get; set; } = 1.85f;
	[Property] public float JumpCutMultiplier { get; set; } = 0.45f;
	[Property] public float ApexHangGravityMultiplier { get; set; } = 0.72f;
	[Property] public float ApexHangVelocity { get; set; } = 60.0f;
	[Property] public float MaxFallSpeed { get; set; } = 940.0f;
	[Property] public float GroundStickVelocity { get; set; } = -8.0f;
	[Property] public bool UseCharacterControllerPunchForJump { get; set; } = true;
	[Property] public float JumpUngroundNudge { get; set; } = 2.0f;
	[Property] public float PostJumpGroundGraceTime { get; set; } = 0.12f;
	[Property] public float CoyoteTime { get; set; } = 0.12f;
	[Property] public float JumpBufferTime { get; set; } = 0.14f;
	[Property] public int ExtraAirJumps { get; set; } = 0;

	[Header( "Camera" )]
	[Property] public bool CameraFollowEnabled { get; set; } = true;
	[Property] public GameObject CameraTarget { get; set; }
	[Property] public Vector3 CameraOffset { get; set; } = new Vector3( 0.0f, -620.0f, 140.0f );
	[Property] public Vector3 CameraFocusOffset { get; set; } = new Vector3( 0.0f, 0.0f, 48.0f );
	[Property] public bool CameraLooksAtPlayer { get; set; } = true;
	[Property] public float CameraFollowSharpness { get; set; } = 12.0f;
	[Property] public float CameraLookAhead { get; set; } = 45.0f;
	[Property] public float CameraLookAheadSharpness { get; set; } = 10.0f;
	[Property] public float CameraVerticalLookAhead { get; set; } = 24.0f;
	[Property] public float CameraDeadZoneX { get; set; } = 4.0f;
	[Property] public float CameraDeadZoneZ { get; set; } = 4.0f;
	[Property] public bool SnapCameraOnStart { get; set; } = true;

	[Header( "Mouse Camera" )]
	[Property] public bool MouseCameraEnabled { get; set; } = true;
	[Property] public Vector2 MouseCameraMaxOffset { get; set; } = new Vector2( 260.0f, 145.0f );
	[Property] public Vector2 MouseCameraScreenDeadZone { get; set; } = new Vector2( 0.06f, 0.08f );
	[Property] public float MouseCameraSharpness { get; set; } = 9.0f;
	[Property] public float MouseCameraReturnSharpness { get; set; } = 13.0f;
	[Property] public bool ShowScreenReticle { get; set; } = true;
	[Property] public bool HideSystemCursorForReticle { get; set; } = true;
	[Property] public PlatformerAimReticle AimReticle { get; set; }

	[Header( "2.5D Lock" )]
	[Property] public bool LockDepthAxis { get; set; } = true;
	[Property] public float LockedDepthY { get; set; } = 0.0f;

	[Header( "Visuals" )]
	[Property] public bool FaceMousePosition { get; set; } = true;
	[Property] public Vector3 MouseLookOriginOffset { get; set; } = new Vector3( 0.0f, 0.0f, 48.0f );
	[Property] public float MouseFacingDeadZone { get; set; } = 2.0f;
	[Property] public bool FaceMoveDirection { get; set; } = true;
	[Property] public float VisualTurnSharpness { get; set; } = 18.0f;
	[Property] public GameObject VisualRoot { get; set; }
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }

	[Header( "Citizen Animation" )]
	[Property] public bool UseCitizenAnimation { get; set; } = true;
	[Property] public CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] public bool TriggerCitizenJumpAnimation { get; set; } = true;
	[Property] public float AnimationSpeedDeadZone { get; set; } = 6.0f;
	[Property] public float AnimationDuckLerpSpeed { get; set; } = 10.0f;
	[Property] public bool AutoAssignCitizenAnimationTarget { get; set; } = true;

	[Header( "Weapons" )]
	[Property] public PlatformerWeapon StartingWeapon { get; set; }
	[Property] public GameObject StartingWeaponPrefab { get; set; }
	[Property] public PlatformerMeleeWeapon DefaultMeleeWeapon { get; set; }
	[Property] public bool AutoCreateDefaultMeleeWeapon { get; set; } = true;

	[Header( "Respawn" )]
	[Property] public bool RespawnWhenFalling { get; set; } = true;
	[Property] public float KillZ { get; set; } = -500.0f;
	[Property] public Vector3 RespawnPoint { get; set; } = Vector3.Zero;
	[Property] public float SafeGroundRespawnHeight { get; set; } = 48.0f;

	public bool IsGrounded => Controller is not null && Controller.IsOnGround && postJumpGroundGraceTimer <= 0.0f;
	public Vector3 Velocity => Controller?.Velocity ?? Vector3.Zero;
	public float MoveInput => moveInput;
	public float Facing => facing;
	public PlatformerWeapon EquippedWeapon => equippedWeapon;
	public bool HasMouseAimPoint => hasMouseWorldSample;
	public Vector3 MouseAimPoint => mouseWorldPoint;
	public bool IsRunActive => IsRunHeld && MathF.Abs( moveInput ) > InputDeadZone;
	public bool IsJumpHeld => jumpHeld;
	public bool IsDuckHeld => duckHeld;
	public bool CanBufferedJump => jumpBufferTimer > 0.0f && (coyoteTimer > 0.0f || airJumpsRemaining > 0);

	private float coyoteTimer;
	private float jumpBufferTimer;
	private int airJumpsRemaining;
	private bool jumpHeld;
	private bool duckHeld;
	private bool jumpWasCut;
	private float postJumpGroundGraceTimer;
	private bool hasMovedCameraOnce;
	private float moveInput;
	private float facing = 1.0f;
	private float smoothedCameraLookAhead;
	private float animationDuckLevel;
	private Vector3 lastSafeGroundPosition;
	private Vector3 lookDirection = Vector3.Forward;
	private Vector2 mouseScreenPosition;
	private Vector2 mouseScreenDelta;
	private Vector3 mouseWorldPoint;
	private Vector3 smoothedMouseCameraOffset;
	private bool hasMouseScreenSample;
	private bool hasMouseWorldSample;
	private GameObject runtimeReticleObject;
	private GameObject runtimeWeaponObject;
	private PlatformerWeapon equippedWeapon;

	private bool IsRunHeld => !string.IsNullOrWhiteSpace( RunAction ) && Input.Down( RunAction );

	protected override void OnStart()
	{
		Controller ??= GameObject.Components.Get<CharacterController>();
		BodyRenderer ??= GameObject.GetComponentInChildren<SkinnedModelRenderer>();
		AnimationHelper ??= GameObject.GetComponentInChildren<CitizenAnimationHelper>();

		if ( Controller is null )
		{
			Log.Warning( $"{nameof( PlatformerPlayerController )} needs a CharacterController on {GameObject.Name}." );
			Enabled = false;
			return;
		}

		ConfigureCitizenAnimation();

		LockedDepthY = GameObject.WorldPosition.y;
		RespawnPoint = GameObject.WorldPosition;
		lastSafeGroundPosition = RespawnPoint;
		airJumpsRemaining = ExtraAirJumps;
		smoothedCameraLookAhead = facing * CameraLookAhead;
		EnsureAimReticle();
		EquipStartingWeapon();

		if ( SnapCameraOnStart )
		{
			SnapCamera();
		}
	}

	protected override void OnDisabled()
	{
		if ( AimReticle is not null )
		{
			AimReticle.IsVisible = false;
		}

		Mouse.Visibility = MouseVisibility.Auto;
	}

	protected override void OnDestroy()
	{
		if ( runtimeReticleObject is not null && runtimeReticleObject.IsValid() )
		{
			runtimeReticleObject.Destroy();
			runtimeReticleObject = null;
		}

		if ( runtimeWeaponObject is not null && runtimeWeaponObject.IsValid() )
		{
			runtimeWeaponObject.Destroy();
			runtimeWeaponObject = null;
		}
	}

	protected override void OnUpdate()
	{
		moveInput = GetMoveInput();
		jumpHeld = IsActionDown( JumpAction );
		duckHeld = IsActionDown( DuckAction );

		if ( IsActionPressed( JumpAction ) )
		{
			jumpBufferTimer = JumpBufferTime;
		}

		if ( IsActionReleased( JumpAction ) )
		{
			TryCutJumpShort();
		}

		UpdateMouseSample();
		UpdateFacing();
		UpdateWeaponInput();
		UpdateCamera( false );
		UpdateReticle();
		UpdateCitizenAnimation();
	}

	protected override void OnFixedUpdate()
	{
		if ( Controller is null )
			return;

		UpdateTimers();
		ApplyHorizontalMovement();
		ApplyGravity();
		TryJump();
		LockDepth();

		Controller.Move();
		UpdateSafeGroundPosition();
		CheckRespawn();
	}

	public void Respawn()
	{
		GameObject.WorldPosition = RespawnPoint;
		Controller.Velocity = Vector3.Zero;
		coyoteTimer = 0.0f;
		jumpBufferTimer = 0.0f;
		airJumpsRemaining = ExtraAirJumps;
		jumpWasCut = false;
		postJumpGroundGraceTimer = 0.0f;
		SnapCamera();
	}

	public void SetRespawnPoint( Vector3 position )
	{
		RespawnPoint = position;
		lastSafeGroundPosition = position;
	}

	public bool EquipWeapon( PlatformerWeapon weapon )
	{
		if ( weapon is null )
			return false;

		if ( equippedWeapon == weapon )
		{
			ApplyWeaponAnimationState( weapon, true );
			return true;
		}

		equippedWeapon?.Unequip();
		equippedWeapon = weapon;
		equippedWeapon.Equip( this );
		ApplyWeaponAnimationState( equippedWeapon, true );
		return true;
	}

	public void UnequipWeapon()
	{
		equippedWeapon?.Unequip();
		equippedWeapon = null;
		ApplyWeaponAnimationState( null, false );
	}

	public Vector3 GetWeaponAttackOrigin()
	{
		return GameObject.WorldPosition + MouseLookOriginOffset;
	}

	public Vector3 GetWeaponAimDirection()
	{
		var direction = lookDirection;
		direction.y = 0.0f;

		if ( direction.LengthSquared <= 0.001f )
		{
			direction = GetFacingRotation().Forward;
			direction.y = 0.0f;
		}

		return direction.LengthSquared <= 0.001f ? Vector3.Forward : direction.Normal;
	}

	private float GetMoveInput()
	{

		var input = UseAnalogMoveInput ? Input.AnalogMove.x : 0.0f;

		if ( IsActionDown( RightAction ) ) input += 1.0f;
		if ( IsActionDown( LeftAction ) ) input -= 1.0f;

		input = Math.Clamp( input, -1.0f, 1.0f );
		return MathF.Abs( input ) < InputDeadZone ? 0.0f : input;
	}

	private void UpdateTimers()
	{
		jumpBufferTimer = MathF.Max( 0.0f, jumpBufferTimer - Time.Delta );
		postJumpGroundGraceTimer = MathF.Max( 0.0f, postJumpGroundGraceTimer - Time.Delta );

		if ( Controller.IsOnGround && postJumpGroundGraceTimer <= 0.0f )
		{
			coyoteTimer = CoyoteTime;
			airJumpsRemaining = ExtraAirJumps;
			jumpWasCut = false;
		}
		else
		{
			coyoteTimer = MathF.Max( 0.0f, coyoteTimer - Time.Delta );
		}
	}

	private void ApplyHorizontalMovement()
	{
		var velocity = Controller.Velocity;
		var targetSpeed = IsRunHeld ? RunSpeed : WalkSpeed;
		var targetX = moveInput * targetSpeed;
		var acceleration = Controller.IsOnGround ? GroundAcceleration : AirAcceleration;

		if ( MathF.Abs( moveInput ) > 0.01f && MathF.Sign( targetX ) != MathF.Sign( velocity.x ) && MathF.Abs( velocity.x ) > 0.01f )
		{
			acceleration *= TurnAroundMultiplier;
		}

		velocity.x = MoveTowards( velocity.x, targetX, acceleration * targetSpeed * Time.Delta );

		if ( Controller.IsOnGround && MathF.Abs( moveInput ) < 0.01f )
		{
			velocity.x = MoveTowards( velocity.x, 0.0f, GroundFriction * targetSpeed * Time.Delta );
		}

		velocity.y = 0.0f;
		Controller.Velocity = velocity;
	}

	private void EquipStartingWeapon()
	{
		var weapon = StartingWeapon ?? DefaultMeleeWeapon;

		if ( weapon is null )
		{
			weapon = CreateWeaponFromPrefab();
		}

		weapon ??= GameObject.Components.GetAll().OfType<PlatformerWeapon>().FirstOrDefault();

		if ( weapon is null && AutoCreateDefaultMeleeWeapon )
		{
			DefaultMeleeWeapon = GameObject.Components.Create<PlatformerMeleeWeapon>();
			weapon = DefaultMeleeWeapon;
		}

		if ( weapon is not null )
		{
			EquipWeapon( weapon );
		}
	}

	private PlatformerWeapon CreateWeaponFromPrefab()
	{
		if ( !StartingWeaponPrefab.IsValid() )
			return null;

		if ( runtimeWeaponObject is not null && runtimeWeaponObject.IsValid() )
		{
			runtimeWeaponObject.Destroy();
		}

		runtimeWeaponObject = StartingWeaponPrefab.Clone( WorldTransform, name: $"{StartingWeaponPrefab.Name} Equipped" );
		runtimeWeaponObject.Parent = GameObject;
		runtimeWeaponObject.Enabled = true;

		var weapon = runtimeWeaponObject.Components.GetAll().OfType<PlatformerWeapon>().FirstOrDefault();

		if ( weapon is null )
		{
			var melee = runtimeWeaponObject.Components.Create<PlatformerMeleeWeapon>();
			melee.CreateRuntimeVisual = false;
			melee.WeaponVisual = runtimeWeaponObject;
			weapon = melee;
		}
		else if ( weapon is PlatformerMeleeWeapon melee && !melee.WeaponVisual.IsValid() )
		{
			melee.CreateRuntimeVisual = false;
			melee.WeaponVisual = runtimeWeaponObject;
		}

		return weapon;
	}

	private void UpdateWeaponInput()
	{
		if ( equippedWeapon is null )
			return;

		if ( IsActionPressed( AttackAction ) )
		{
			equippedWeapon.TryPrimaryAttack();
		}
	}

	private void ApplyGravity()
	{
		var velocity = Controller.Velocity;

		if ( Controller.IsOnGround && postJumpGroundGraceTimer <= 0.0f && velocity.z <= 0.0f )
		{
			velocity.z = GroundStickVelocity;
		}
		else
		{
			var gravityScale = 1.0f;

			if ( velocity.z < 0.0f )
			{
				gravityScale = FallGravityMultiplier;
			}
			else if ( velocity.z > 0.0f && !jumpHeld )
			{
				gravityScale = LowJumpGravityMultiplier;
			}
			else if ( MathF.Abs( velocity.z ) <= ApexHangVelocity )
			{
				gravityScale = ApexHangGravityMultiplier;
			}

			velocity.z -= Gravity * gravityScale * Time.Delta;
			velocity.z = MathF.Max( velocity.z, -MaxFallSpeed );
		}

		Controller.Velocity = velocity;
	}

	private void TryJump()
	{
		if ( jumpBufferTimer <= 0.0f )
			return;

		if ( coyoteTimer > 0.0f )
		{
			DoJump();
			coyoteTimer = 0.0f;
			return;
		}

		if ( airJumpsRemaining > 0 )
		{
			DoJump();
			airJumpsRemaining--;
		}
	}

	private void DoJump()
	{
		var velocity = Controller.Velocity;
		velocity.z = 0.0f;
		Controller.Velocity = velocity;

		if ( UseCharacterControllerPunchForJump )
		{
			Controller.Punch( Vector3.Up * JumpSpeed );
		}
		else
		{
			velocity.z = JumpSpeed;
			Controller.Velocity = velocity;
		}

		if ( JumpUngroundNudge > 0.0f )
		{
			GameObject.WorldPosition += Vector3.Up * JumpUngroundNudge;
		}

		jumpBufferTimer = 0.0f;
		jumpWasCut = false;
		postJumpGroundGraceTimer = PostJumpGroundGraceTime;

		if ( TriggerCitizenJumpAnimation )
		{
			AnimationHelper?.TriggerJump();
		}
	}

	private void TryCutJumpShort()
	{
		if ( Controller is null || jumpWasCut )
			return;

		var velocity = Controller.Velocity;

		if ( velocity.z <= 0.0f )
			return;

		velocity.z *= JumpCutMultiplier;
		Controller.Velocity = velocity;
		jumpWasCut = true;
	}

	private void LockDepth()
	{
		if ( !LockDepthAxis )
			return;

		var position = GameObject.WorldPosition;
		position.y = LockedDepthY;
		GameObject.WorldPosition = position;

		var velocity = Controller.Velocity;
		velocity.y = 0.0f;
		Controller.Velocity = velocity;
	}

	private void UpdateFacing()
	{
		var hasMouseLook = FaceMousePosition && TryUpdateMouseLookDirection();

		if ( !hasMouseLook )
		{
			if ( MathF.Abs( moveInput ) >= 0.01f )
			{
				facing = MathF.Sign( moveInput );
			}

			lookDirection = GetFacingRotation().Forward;
		}

		if ( !hasMouseLook && !FaceMoveDirection )
			return;

		var target = VisualRoot ?? GameObject;
		target.WorldRotation = GetFacingRotation();
	}

	private bool TryUpdateMouseLookDirection()
	{
		if ( !hasMouseWorldSample )
			return false;

		var lookOrigin = GameObject.WorldPosition + MouseLookOriginOffset;
		var direction = mouseWorldPoint - lookOrigin;
		direction.y = 0.0f;

		if ( direction.LengthSquared <= 0.001f )
			return false;

		lookDirection = direction.Normal;

		if ( MathF.Abs( direction.x ) > MouseFacingDeadZone )
		{
			facing = MathF.Sign( direction.x );
		}

		return true;
	}

	private void UpdateMouseSample()
	{
		hasMouseScreenSample = TryGetMouseScreenSample( out mouseScreenPosition, out mouseScreenDelta );
		hasMouseWorldSample = hasMouseScreenSample && TryGetMousePointOnGameplayPlane( mouseScreenPosition, out mouseWorldPoint );
	}

	private bool TryGetMouseScreenSample( out Vector2 position, out Vector2 delta )
	{
		position = default;
		delta = default;

		var screenSize = Screen.Size;

		if ( screenSize.x <= 1.0f || screenSize.y <= 1.0f )
			return false;

		position = new Vector2(
			Math.Clamp( Mouse.Position.x, 0.0f, screenSize.x - 1.0f ),
			Math.Clamp( Mouse.Position.y, 0.0f, screenSize.y - 1.0f )
		);

		var center = screenSize * 0.5f;
		var halfWidth = MathF.Max( center.x, 1.0f );
		var halfHeight = MathF.Max( center.y, 1.0f );

		delta = new Vector2(
			Math.Clamp( (position.x - center.x) / halfWidth, -1.0f, 1.0f ),
			Math.Clamp( (center.y - position.y) / halfHeight, -1.0f, 1.0f )
		);

		return true;
	}

	private bool TryGetMousePointOnGameplayPlane( Vector2 screenPosition, out Vector3 point )
	{
		point = default;

		var camera = CameraTarget?.Components.Get<CameraComponent>() ?? Scene?.Camera;

		if ( camera is null )
			return false;

		var ray = camera.ScreenPixelToRay( screenPosition );

		if ( MathF.Abs( ray.Forward.y ) <= 0.0001f )
			return false;

		var distance = (LockedDepthY - ray.Position.y) / ray.Forward.y;

		if ( distance < 0.0f )
			return false;

		point = ray.Project( distance );
		point.y = LockedDepthY;
		return true;
	}

	private Rotation GetFacingRotation()
	{
		return Rotation.FromYaw( facing > 0.0f ? 0.0f : 180.0f );
	}

	private void ConfigureCitizenAnimation()
	{
		if ( !UseCitizenAnimation || AnimationHelper is null )
			return;

		if ( AutoAssignCitizenAnimationTarget && BodyRenderer is not null )
		{
			AnimationHelper.Target = BodyRenderer;
		}

		AnimationHelper.IsGrounded = IsGrounded;
		AnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		AnimationHelper.WithLook( GetFacingRotation().Forward );
	}

	private void UpdateCitizenAnimation()
	{
		if ( !UseCitizenAnimation || AnimationHelper is null || Controller is null )
			return;

		if ( AutoAssignCitizenAnimationTarget && AnimationHelper.Target is null && BodyRenderer is not null )
		{
			AnimationHelper.Target = BodyRenderer;
		}

		var targetSpeed = IsRunActive ? RunSpeed : WalkSpeed;
		var wishVelocity = new Vector3( moveInput * targetSpeed, 0.0f, 0.0f );

		if ( MathF.Abs( wishVelocity.x ) < AnimationSpeedDeadZone )
		{
			wishVelocity.x = 0.0f;
		}

		animationDuckLevel = MoveTowards( animationDuckLevel, duckHeld ? 1.0f : 0.0f, AnimationDuckLerpSpeed * Time.Delta );

		AnimationHelper.WithWishVelocity( wishVelocity );
		AnimationHelper.WithVelocity( Controller.Velocity );
		AnimationHelper.WithLook( lookDirection );
		AnimationHelper.IsGrounded = IsGrounded;
		AnimationHelper.MoveStyle = IsRunActive ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
		AnimationHelper.DuckLevel = animationDuckLevel;
	}

	public void ApplyWeaponAnimationState( PlatformerWeapon weapon, bool triggerDeploy )
	{
		if ( !UseCitizenAnimation || AnimationHelper is null || !AnimationHelper.Target.IsValid() )
			return;

		AnimationHelper.HoldType = weapon?.CitizenHoldType ?? CitizenAnimationHelper.HoldTypes.None;
		AnimationHelper.Handedness = weapon?.CitizenHandedness ?? CitizenAnimationHelper.Hand.Right;
		AnimationHelper.IsWeaponLowered = weapon is null;

		if ( weapon is not null && triggerDeploy )
		{
			AnimationHelper.TriggerDeploy();
		}
	}

	public void TriggerWeaponAttackAnimation( CitizenAnimationHelper.HoldTypes holdType )
	{
		if ( !UseCitizenAnimation || AnimationHelper is null || !AnimationHelper.Target.IsValid() )
			return;

		AnimationHelper.HoldType = holdType;
		AnimationHelper.IsWeaponLowered = false;
		AnimationHelper.Target.Set( "b_attack", true );
		AnimationHelper.Target.Set( "b_attack_primary", true );
	}

	public void SetWeaponAttackAnimationState( bool attacking, float progress )
	{
		if ( !UseCitizenAnimation || AnimationHelper is null || !AnimationHelper.Target.IsValid() )
			return;

		AnimationHelper.Target.Set( "b_attack", attacking );
		AnimationHelper.Target.Set( "b_attack_primary", attacking );
		AnimationHelper.Target.Set( "attack", Math.Clamp( progress, 0.0f, 1.0f ) );
		AnimationHelper.Target.Set( "attack_progress", Math.Clamp( progress, 0.0f, 1.0f ) );
	}

	public void ClearWeaponAttackAnimation()
	{
		SetWeaponAttackAnimationState( false, 0.0f );
	}

	private void UpdateCamera( bool snap )
	{
		if ( !CameraFollowEnabled || CameraTarget is null )
			return;

		var lookT = snap ? 1.0f : 1.0f - MathF.Exp( -CameraLookAheadSharpness * Time.Delta );
		smoothedCameraLookAhead += (facing * CameraLookAhead - smoothedCameraLookAhead) * lookT;
		var hasMouseCamera = MouseCameraEnabled && hasMouseScreenSample;
		var mouseCameraOffset = UpdateMouseCameraOffset( hasMouseCamera ? GetMouseCameraTargetOffset() : Vector3.Zero, snap );

		var verticalVelocity = Controller?.Velocity.z ?? 0.0f;
		var verticalLookAhead = Math.Clamp( verticalVelocity * 0.05f, -CameraVerticalLookAhead, CameraVerticalLookAhead );
		var horizontalFocusOffset = hasMouseCamera ? mouseCameraOffset : new Vector3( smoothedCameraLookAhead, 0.0f, 0.0f );
		var desiredFocus = GameObject.WorldPosition + CameraFocusOffset + horizontalFocusOffset + new Vector3( 0.0f, 0.0f, verticalLookAhead );
		var desiredPosition = desiredFocus + CameraOffset;

		if ( !snap && hasMovedCameraOnce )
		{
			var delta = desiredPosition - CameraTarget.WorldPosition;

			if ( MathF.Abs( delta.x ) < CameraDeadZoneX ) desiredPosition.x = CameraTarget.WorldPosition.x;
			if ( MathF.Abs( delta.z ) < CameraDeadZoneZ ) desiredPosition.z = CameraTarget.WorldPosition.z;
		}

		if ( snap || CameraFollowSharpness <= 0.0f || !hasMovedCameraOnce )
		{
			CameraTarget.WorldPosition = desiredPosition;
			hasMovedCameraOnce = true;
			UpdateCameraRotation( desiredFocus );
			return;
		}

		var t = 1.0f - MathF.Exp( -CameraFollowSharpness * Time.Delta );
		CameraTarget.WorldPosition += (desiredPosition - CameraTarget.WorldPosition) * t;
		UpdateCameraRotation( desiredFocus );
	}

	private Vector3 GetMouseCameraTargetOffset()
	{
		var cameraDelta = new Vector2(
			ApplyDeadZone( mouseScreenDelta.x, MouseCameraScreenDeadZone.x ),
			ApplyDeadZone( mouseScreenDelta.y, MouseCameraScreenDeadZone.y )
		);

		return new Vector3(
			cameraDelta.x * MathF.Max( 0.0f, MouseCameraMaxOffset.x ),
			0.0f,
			cameraDelta.y * MathF.Max( 0.0f, MouseCameraMaxOffset.y )
		);
	}

	private Vector3 UpdateMouseCameraOffset( Vector3 targetOffset, bool snap )
	{
		if ( snap )
		{
			smoothedMouseCameraOffset = targetOffset;
			return smoothedMouseCameraOffset;
		}

		var sharpness = targetOffset.LengthSquared < smoothedMouseCameraOffset.LengthSquared
			? MouseCameraReturnSharpness
			: MouseCameraSharpness;

		if ( sharpness <= 0.0f )
		{
			smoothedMouseCameraOffset = targetOffset;
			return smoothedMouseCameraOffset;
		}

		var t = 1.0f - MathF.Exp( -sharpness * Time.Delta );
		smoothedMouseCameraOffset += (targetOffset - smoothedMouseCameraOffset) * t;
		return smoothedMouseCameraOffset;
	}

	private void EnsureAimReticle()
	{
		if ( AimReticle is not null || !ShowScreenReticle )
			return;

		runtimeReticleObject = new GameObject( true, "Platformer Aim Reticle" );
		runtimeReticleObject.GetOrAddComponent<ScreenPanel>();
		AimReticle = runtimeReticleObject.GetOrAddComponent<PlatformerAimReticle>();
	}

	private void UpdateReticle( bool allowVisible = true )
	{
		if ( !ShowScreenReticle )
		{
			if ( AimReticle is not null )
			{
				AimReticle.IsVisible = false;
			}

			return;
		}

		EnsureAimReticle();

		if ( HideSystemCursorForReticle )
		{
			Mouse.Visibility = MouseVisibility.Auto;
		}

		if ( AimReticle is null )
			return;

		AimReticle.Position = mouseScreenPosition;
		AimReticle.IsVisible = allowVisible && hasMouseScreenSample;
	}

	private void UpdateCameraRotation( Vector3 focusPoint )
	{
		if ( !CameraLooksAtPlayer || CameraTarget is null )
			return;

		var direction = focusPoint - CameraTarget.WorldPosition;

		if ( direction.Length <= 0.001f )
			return;

		CameraTarget.WorldRotation = Rotation.LookAt( direction.Normal, Vector3.Up );
	}

	private void SnapCamera()
	{
		UpdateCamera( true );
	}

	private void UpdateSafeGroundPosition()
	{
		if ( Controller.IsOnGround )
		{
			lastSafeGroundPosition = GameObject.WorldPosition;
		}
	}

	private void CheckRespawn()
	{
		if ( !RespawnWhenFalling || GameObject.WorldPosition.z > KillZ )
			return;

		RespawnPoint = lastSafeGroundPosition + Vector3.Up * SafeGroundRespawnHeight;
		Respawn();
	}

	private static bool IsActionDown( string actionName )
	{
		return !string.IsNullOrWhiteSpace( actionName ) && Input.Down( actionName );
	}

	private static bool IsActionPressed( string actionName )
	{
		return !string.IsNullOrWhiteSpace( actionName ) && Input.Pressed( actionName );
	}

	private static bool IsActionReleased( string actionName )
	{
		return !string.IsNullOrWhiteSpace( actionName ) && Input.Released( actionName );
	}

	private static float MoveTowards( float current, float target, float maxDelta )
	{
		if ( MathF.Abs( target - current ) <= maxDelta )
			return target;

		return current + MathF.Sign( target - current ) * maxDelta;
	}

	private static float ApplyDeadZone( float value, float deadZone )
	{
		deadZone = Math.Clamp( deadZone, 0.0f, 0.99f );
		var magnitude = MathF.Abs( value );

		if ( magnitude <= deadZone )
			return 0.0f;

		return MathF.Sign( value ) * Math.Clamp( (magnitude - deadZone) / (1.0f - deadZone), 0.0f, 1.0f );
	}
}
