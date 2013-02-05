using System;
using System.Linq;
using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class UnitModel : WorldObject {

	internal SkinnedMeshRenderer meshRenderer;
	internal UnitAnimator animator;

	public Transform mainHand;
	public Transform offHand;
	public Transform head;
	public Transform torso;

	internal Weapon currentWeapon;

	internal Clip currentClip = 0;

	internal UnitModelPosture posture = UnitModelPosture.Normal;
	internal string idleClip = UnitAnimation.IDLE;

	public bool adjescentEnemy;
	public bool actionSide;
	public bool idle;

	public GridTile currentSmallestCover = null;
	public Unit primaryEnemy = null;

	private Unit owner;
	public Holders equiomentHolders;
	public UnitMaterialManager materialManager;

	public void Init( Unit unit ) {
		this.animator = GetComponent<UnitAnimator>();
		this.owner = unit;
		this.meshRenderer = renderers[0] as SkinnedMeshRenderer;
		this.materialManager = new UnitMaterialManager( this );
		//this.equiomentHolders = new Holders();
	}

	internal void SetMaterial( Material material ) {
		meshRenderer.material = material;
	}

	internal void SetTempMaterial( Material material ) {
		meshRenderer.material = material;
	}

	internal void SetPosture( UnitModelPosture posture ) {

		this.posture = posture;

		if( adjescentEnemy && owner.selected && owner.currentWeapon.melee ) {
			idleClip = UnitAnimation.IDLE;
			//if( owner.relations.primaryEnemy ) {
			//    owner.transform.LookAt( owner.relations.primaryEnemy.transform );
			//}
		} else

		switch( posture ) {
			case UnitModelPosture.Normal:
				idleClip = UnitAnimation.IDLE;
				break;
			case UnitModelPosture.CoverWall:
				idleClip = actionSide ? UnitAnimation.COVER_WALL_R : UnitAnimation.COVER_WALL_L;
				break;
			case UnitModelPosture.CoverDucked:
				idleClip = actionSide ? UnitAnimation.COVER_DUCKED_R : UnitAnimation.COVER_DUCKED_L;
				break;
			default:
				Debug.LogWarning( "Unknown posture - " + posture );
				break;
		}

	}

	internal void UpdatePosture() {

		//if( currentSmallestCover == null ) {
		//	SetPosture( UnitModelPosture.Normal );
		//	if( owner.relations.primaryEnemy ) {
		//		owner.transform.LookAt( owner.relations.primaryEnemy.transform );
		//	}
		//} else {
		//	if( currentSmallestCover.coverValue < owner.props.size ) {
		//		SetPosture( UnitModelPosture.CoverDucked );
		//	} else {
		//		SetPosture( UnitModelPosture.CoverWall );
		//	}
		//	owner.transform.LookAt( currentSmallestCover.transform );
		//}

		SetPosture( UnitModelPosture.Normal );
		if( owner.relations.primaryEnemy ) {
			owner.transform.LookAt( owner.relations.primaryEnemy.transform );
		}

	}

	internal void RefreshPosture() {
		SetPosture( posture );
	}

	internal void Equip( Equippable item, Transform hand = null ) {
		hand = hand ?? mainHand;
		item.transform.AttachTo( hand );
		//item.transform.position = hand.transform.position;
		//item.transform.rotation = hand.transform.rotation;
		//item.transform.parent = hand;
	}
	internal void Hide( Equippable item ) {

		//	weapon.model.enabled = false;

		if( item.equipmentType == EquipmentType.Hidden ) {
			item.gameObject.SetActive( false );
		} else {
			item.transform.AttachTo( equiomentHolders.NextAvailable( item.equipmentType ) );
		}

		//if( item.animation != null && item.animation.GetClip( "load" ) != null ) {
		//	item.animation.Play( "load" );
		//}

	}
	internal void Show( Equippable item ) {

		item.transform.AttachTo( mainHand );
		//item.gameObject.SetActive( true );

		//if( item.animation != null && item.animation.GetClip( "load" ) != null ) {
		//	item.animation.Play( "load" );
		//}

	}

	public void Aim() {
		animator.aiming = true;
	}

	public void Attack( Unit unit, bool hit ) {
		animator.QuickSetRevert( "shoot" );
	}

	public void Damage() {
		animator.QuickSetRevert("damage");
		BloodyUp();
	}

	public void Die() {
		animator.dead = true;
		BloodyUp();
	}

	public void Reload() {

		animator.QuickSetRevert( "reload" );

		//if( posture.Equals( UnitModelPosture.Normal ) || GameMode.Is( GameModes.PickUnit ) ) {
		//	return;
		//}

		//if( posture.Equals( UnitModelPosture.CoverDucked ) ) {
		//	return;
		//}

	}



	public Material bloodyMat;
	public void BloodyUp() {

		List<Material> list = new List<Material>();

		list.AddRange( meshRenderer.materials );

		Material m = new Material( bloodyMat ) { mainTextureOffset = new Vector2( rand , rand ) };

		list.Add( m );

		meshRenderer.materials = list.ToArray();

	}

	[Serializable]
	public class Holders {

		public Transform[] holsters;
		public Transform[] backFirearm;
		public Transform[] backMelee;
		public Transform[] misc;

		public Transform NextAvailable( EquipmentType type ) {

			Transform[] holders = GetGroupFor( type );

			foreach( Transform t in holders.Where( t => t.childCount == 0 ) ) {
				return t;
			}

			return holders[0];

			//return holsters[0];

		}

		private Transform[] GetGroupFor( EquipmentType type ) {

			switch( type ) {
				case EquipmentType.SideArm:
				case EquipmentType.Claws:
					return holsters;
				case EquipmentType.Sword:
					return backMelee;
				case EquipmentType.Rifle:
					return backFirearm;
				case EquipmentType.Misc:
					return misc;
				default:
					throw new ArgumentOutOfRangeException( "type" );
			}
		}

	}

}