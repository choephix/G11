using UnityEngine;
using System.Collections;

public class Firearm : Weapon {

	public Transform ammoHolder;
	public Transform barrel;
	public LineRenderer shotLine;

	public AmmoType ammoType = AmmoType.BulletSmall;
	public int baseAmmoClipSize = 8;
	public float accuracy = 100f; //TODO not implemented

	// CHANGABLES

	public Ammo ammoClip;
	internal int ammoLeft;

	// PROPERTY GETTERS

	internal int ammoClipSize { get { return baseAmmoClipSize; } }
	internal new float damage { get { return baseDamage * ammoClip.damageMultiplier; } }
	internal new DamageType damageType { get { return ammoClip.damageType; } }

	internal bool haveAmmo { get { return ammoLeft>0; } }
	internal new bool canAttack { get { return haveAmmo; } }

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

	public override void Attack( Unit targetUnit, IDamageable hittee ) {

		ammoLeft--;

		shotLine.enabled = true;
		shotLine.SetPosition( 0, barrel.position );

		if( hittee is Unit ) {
			shotLine.SetPosition( 1, targetUnit.spots.torso.position );
		} else if( hittee is Obstruction ) {
			shotLine.SetPosition( 1, ( hittee as Obstruction ).transform.position );
		} else {
			shotLine.SetPosition( 1, targetUnit.spots.torso.position + BaseClass.Randomize( Vector3.one ) );
		}

		StartCoroutine( AttackCoroutine() );

		animation.Play( "fire" );

	}


	internal void Reload() {
		ammoLeft = ammoClipSize;
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
