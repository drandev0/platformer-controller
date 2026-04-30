using System;
using System.Linq;
using Sandbox.Citizen;

public sealed class PlatformerMeleeWeapon : PlatformerWeapon
{
	[Header( "Attack" )]
	[Property] public float Damage { get; set; } = 25.0f;
	[Property] public float Range { get; set; } = 86.0f;
	[Property] public float Radius { get; set; } = 18.0f;
	[Property] public float Cooldown { get; set; } = 0.42f;
	[Property] public float ImpactDelay { get; set; } = 0.11f;
	[Property] public float AttackAnimationDuration { get; set; } = 0.34f;
	[Property] public float HitForce { get; set; } = 420.0f;
	[Property] public bool LogHits { get; set; } = false;

	[Header( "Visual" )]
	[Property] public bool CreateRuntimeVisual { get; set; } = true;
	[Property] public GameObject WeaponVisual { get; set; }
	[Property] public string VisualModel { get; set; } = "models/dev/box.vmdl";
	[Property] public Vector3 VisualScale { get; set; } = new Vector3( 0.65f, 0.08f, 0.08f );
	[Property] public Vector3 VisualOffset { get; set; } = new Vector3( 26.0f, -10.0f, 46.0f );
	[Property] public float VisualSwingLift { get; set; } = 0.58f;
	[Property] public float VisualSwingReach { get; set; } = 12.0f;

	private float cooldownTimer;
	private float impactTimer;
	private float attackTimer;
	private bool hasPendingImpact;
	private GameObject runtimeVisualObject;

	public PlatformerMeleeWeapon()
	{
		DisplayName = "Melee";
		CitizenHoldType = CitizenAnimationHelper.HoldTypes.Swing;
		CitizenHandedness = CitizenAnimationHelper.Hand.Right;
	}

	protected override void OnUpdate()
	{
		cooldownTimer = MathF.Max( 0.0f, cooldownTimer - Time.Delta );

		if ( attackTimer > 0.0f )
		{
			attackTimer = MathF.Max( 0.0f, attackTimer - Time.Delta );
			Owner?.SetWeaponAttackAnimationState( true, AttackProgress );
		}
		else
		{
			Owner?.SetWeaponAttackAnimationState( false, 0.0f );
		}

		if ( hasPendingImpact )
		{
			impactTimer -= Time.Delta;

			if ( impactTimer <= 0.0f )
			{
				hasPendingImpact = false;
				PerformImpactTrace();
			}
		}

		UpdateVisual();
	}

	public override bool TryPrimaryAttack()
	{
		if ( !IsEquipped || cooldownTimer > 0.0f )
			return false;

		cooldownTimer = MathF.Max( 0.01f, Cooldown );
		impactTimer = Math.Clamp( ImpactDelay, 0.0f, MathF.Max( 0.01f, AttackAnimationDuration ) );
		attackTimer = MathF.Max( 0.01f, AttackAnimationDuration );
		hasPendingImpact = true;

		Owner?.TriggerWeaponAttackAnimation( CitizenHoldType );
		Owner?.SetWeaponAttackAnimationState( true, 0.0f );
		UpdateVisual();
		return true;
	}

	protected override void OnEquipped()
	{
		base.OnEquipped();
		EnsureVisual();
		SetVisualEnabled( true );
	}

	protected override void OnUnequipped()
	{
		SetVisualEnabled( false );
		hasPendingImpact = false;
		attackTimer = 0.0f;
		base.OnUnequipped();
	}

	protected override void OnDestroy()
	{
		if ( runtimeVisualObject is not null && runtimeVisualObject.IsValid() )
		{
			runtimeVisualObject.Destroy();
			runtimeVisualObject = null;
		}
	}

	private float AttackProgress
	{
		get
		{
			if ( AttackAnimationDuration <= 0.0f )
				return 1.0f;

			return Math.Clamp( 1.0f - attackTimer / AttackAnimationDuration, 0.0f, 1.0f );
		}
	}

	private void PerformImpactTrace()
	{
		if ( Owner is null || Scene is null )
			return;

		var start = Owner.GetWeaponAttackOrigin();
		var direction = Owner.GetWeaponAimDirection();

		if ( direction.LengthSquared <= 0.001f )
			return;

		var end = start + direction.Normal * MathF.Max( 0.0f, Range );
		var trace = Scene.Trace
			.Sphere( MathF.Max( 1.0f, Radius ), start, end )
			.IgnoreGameObjectHierarchy( Owner.GameObject )
			.UseHitboxes( true )
			.Run();

		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return;

		var hit = new PlatformerMeleeHit( this, Owner, trace.GameObject, trace, Damage, direction.Normal * HitForce );
		DispatchHit( hit );

		if ( LogHits )
		{
			Log.Info( $"{DisplayName} hit {trace.GameObject.Name} for {Damage}." );
		}
	}

	private void DispatchHit( PlatformerMeleeHit hit )
	{
		var target = hit.Target;

		while ( target.IsValid() )
		{
			foreach ( var receiver in target.Components.GetAll().OfType<IPlatformerMeleeDamageReceiver>() )
			{
				receiver.OnMeleeHit( hit );
			}

			target = target.Parent;
		}
	}

	private void EnsureVisual()
	{
		if ( WeaponVisual.IsValid() || !CreateRuntimeVisual || Owner is null )
			return;

		if ( GameObject != Owner.GameObject )
		{
			WeaponVisual = GameObject;
			return;
		}

		runtimeVisualObject = new GameObject( true, $"{DisplayName} Visual" );
		runtimeVisualObject.Parent = Owner.VisualRoot ?? Owner.GameObject;
		runtimeVisualObject.LocalScale = VisualScale;

		var renderer = runtimeVisualObject.Components.Create<ModelRenderer>();

		if ( !string.IsNullOrWhiteSpace( VisualModel ) )
		{
			renderer.Model = Model.Load( VisualModel );
		}

		WeaponVisual = runtimeVisualObject;
	}

	private void SetVisualEnabled( bool enabled )
	{
		if ( WeaponVisual.IsValid() )
		{
			WeaponVisual.Enabled = enabled;
		}
	}

	private void UpdateVisual()
	{
		if ( !IsEquipped || Owner is null )
			return;

		EnsureVisual();

		if ( !WeaponVisual.IsValid() )
			return;

		var direction = Owner.GetWeaponAimDirection();

		if ( direction.LengthSquared <= 0.001f )
			return;

		direction = direction.Normal;
		var progress = AttackProgress;
		var swing = attackTimer > 0.0f ? MathF.Sin( progress * MathF.PI ) : 0.0f;
		var lift = attackTimer > 0.0f ? VisualSwingLift * MathF.Cos( progress * MathF.PI ) : 0.12f;
		var visualDirection = (direction + Vector3.Up * lift).Normal;
		var origin = Owner.GameObject.WorldPosition + new Vector3( direction.x * VisualOffset.x, VisualOffset.y, VisualOffset.z );

		WeaponVisual.WorldPosition = origin + visualDirection * (VisualOffset.x + swing * VisualSwingReach );
		WeaponVisual.WorldRotation = Rotation.LookAt( visualDirection, Vector3.Up );
		WeaponVisual.LocalScale = VisualScale;
	}
}

public readonly struct PlatformerMeleeHit
{
	public PlatformerMeleeHit( PlatformerMeleeWeapon weapon, PlatformerPlayerController attacker, GameObject target, SceneTraceResult trace, float damage, Vector3 force )
	{
		Weapon = weapon;
		Attacker = attacker;
		Target = target;
		Trace = trace;
		Damage = damage;
		Force = force;
	}

	public PlatformerMeleeWeapon Weapon { get; }
	public PlatformerPlayerController Attacker { get; }
	public GameObject Target { get; }
	public SceneTraceResult Trace { get; }
	public float Damage { get; }
	public Vector3 Force { get; }
}

public interface IPlatformerMeleeDamageReceiver
{
	void OnMeleeHit( PlatformerMeleeHit hit );
}
