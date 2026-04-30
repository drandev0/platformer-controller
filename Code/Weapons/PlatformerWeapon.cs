using Sandbox.Citizen;

public abstract class PlatformerWeapon : Component
{
	[Property] public string DisplayName { get; set; } = "Weapon";
	[Property] public CitizenAnimationHelper.HoldTypes CitizenHoldType { get; set; } = CitizenAnimationHelper.HoldTypes.None;
	[Property] public CitizenAnimationHelper.Hand CitizenHandedness { get; set; } = CitizenAnimationHelper.Hand.Right;

	public PlatformerPlayerController Owner { get; private set; }
	public bool IsEquipped { get; private set; }

	public void Equip( PlatformerPlayerController owner )
	{
		if ( owner is null )
			return;

		if ( IsEquipped && Owner == owner )
			return;

		if ( IsEquipped )
		{
			Unequip();
		}

		Owner = owner;
		IsEquipped = true;
		OnEquipped();
	}

	public void Unequip()
	{
		if ( !IsEquipped )
			return;

		OnUnequipped();
		IsEquipped = false;
		Owner = null;
	}

	public virtual bool TryPrimaryAttack()
	{
		return false;
	}

	protected virtual void OnEquipped()
	{
		Owner?.ApplyWeaponAnimationState( this, true );
	}

	protected virtual void OnUnequipped()
	{
		Owner?.ClearWeaponAttackAnimation();
	}
}
