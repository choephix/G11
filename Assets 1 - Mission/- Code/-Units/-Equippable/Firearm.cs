using UnityEngine;
using System.Collections;

public class Firearm : Weapon {

	public Transform ammoHolder;
	public Transform barrel;
	public LineRenderer shotLine;

	public AmmoType ammoType = AmmoType.bulletSmall;
	public int baseAmmoClipSize = 8;

	// CHANGABLES

	public Ammo ammoClip;
	internal int ammoLeft;

	// PROPERTY GETTERS

	internal int ammoClipSize { get { return baseAmmoClipSize; } }
	internal new float damage { get { return baseDamage * ammoClip.damageMultiplier; } }
	internal new DamageType damageType { get { return ammoClip.damageType; } }

	internal bool haveAmmo { get { return ammoLeft>0; } }
	internal new bool canAttack { get { return haveAmmo; } }

	internal event EventHandler eventReload = delegate { };

	public override void Init( Unit owner ) {

		base.Init( owner );

		actions.Add( new ActionsBook.Reload( owner, this ) );

		if( ammoHolder && ammoClip ) {
			ammoClip = Instantiate( ammoClip, ammoHolder.position, ammoHolder.rotation ) as Ammo;
			ammoClip.transform.parent = ammoHolder;
			ammoClip.transform.localScale = Vector3.one;
		}

		Reload();

		shotLine.enabled = false;
		
	}

	public override bool CanTarget( Unit targetUnit ) {

		return canAttack;

	}

	public override void Attack( Unit targetUnit, bool hit ) {

		ammoLeft--;

		shotLine.enabled = true;
		shotLine.SetPosition( 0, barrel.position );
		shotLine.SetPosition( 1, targetUnit.spots.torso.position +
			( hit ? Vector3.zero : ( BaseClass.Randomize( Vector3.one ) ) ) );
		StartCoroutine( AttackCoroutine() );

		animation.Play( "fire" );

	}


	internal void Reload() {
		ammoLeft = ammoClipSize;
		eventReload.Invoke();
	}

	public IEnumerator AttackCoroutine() {
		yield return new WaitForSeconds( .15f );
		//	OnAttackFinished();
	}

	public void OnAttackFinished() {
		shotLine.enabled = false;
		eventAttackFinished.Invoke(this);
		//	d_OnAttackFinished( this );
	}

}
