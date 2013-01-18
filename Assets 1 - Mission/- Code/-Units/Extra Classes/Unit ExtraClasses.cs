using System.Collections;
using System.Collections.Generic;
using UnityEngine;







public static class UnitMath {
	
}

public static class UnitAnimation {

	//looping
	public const string IDLE	= "idle";
	public const string MOVE	= "run";
	public const string AIM		= "aimed";

	public const string JUMP	= "jumped";

	public const string COVER_DUCKED_L	= "coverDuckedL";
	public const string COVER_DUCKED_R	= "coverDuckedR";
	public const string COVER_WALL_L	= "coverL";
	public const string COVER_WALL_R	= "coverR";

	//onetimers
	public const string ATTACK_RANGED	= "shoot";
	public const string ATTACK_WIELD	= "attackSword";
	public const string ATTACK_FIST		= "attackFist";
	public const string HURT			= "shot";
	public const string DEATH			= "die";
	public const string RELOAD			= "reload";
	public const string RELOAD_DUCKED_L	= "reloadDuckedL";
	public const string RELOAD_DUCKED_R	= "reloadDuckedR";

	public const string JUMP_START	= "jumpStart";
	public const string JUMP_END	= "jumpEnd";

	// DEV

	public const float MELEE_HIT_DELAY = .33f;
	public const float RANGE_HIT_DELAY = .09f;

}

[System.Serializable]
public class UnitTransformSpots {
	public Transform left;
	public Transform right;
	public Transform overHead;
	public Transform wayOverHead;
	public Transform head;
	public Transform torso;
	public Transform feet;
	public Transform back;
	public Transform forth;
	public Transform wayBack;
	public Transform wayForth;

	public Transform upBack;
	public Transform upForth;
	public Transform upLeft;
	public Transform upRight;
}

[System.Serializable]
public class UnitStatus {

	private int _maxHealth;
	private float _health;
	public float health {
		get { return _health; }
		set { _health = value.ClipMaxMin( _maxHealth ); }
	}
	private float _armor;
	public float armor {
		get { return _armor; }
		set { _armor = value > 0 ? value : 0; }
	}

	private int _maxActions;
	public int actionPoints;

	public bool fullHealth { get { return health >= _maxHealth; } }

	internal void Init( UnitProperties props ) {
		_armor = props.armor;
		_health = props.maxHealth;
		_maxHealth = props.maxHealth;
		_maxActions = Config.BASE_UNIT_ACTIONS;
		ResetActions();
	}

	internal void ResetActions() {
		actionPoints = _maxActions;
	}

	internal void ReceiveDamage( float amount, DamageType type ) {
		switch( type ) {
			case DamageType.NORMAL:
				if( armor > 0 ) {
					if( armor >= amount ) {
						armor -= amount;
					} else {
						amount -= armor;
						armor = 0;
						health -= amount;
					}
				} else {
					health -= amount;
				}
				return;
			case DamageType.INTERNAL:
				health -= amount;
				return;
			case DamageType.ANTIARMOR:
				if( armor > 0 ) {
					amount *= 2;
					if( armor >= amount ) {
						armor -= amount;
						return;
					} else {
						amount -= armor;
						armor = 0;
						amount = ( amount - amount % 2 ) / 2;
						health -= amount;
						return;
					}
				} else {
					health -= amount;
				}
				return;
			case DamageType.HEALING:
				health += amount;
				return;
			default:
				return;
		}
	}

}

[System.Serializable]
public class UnitProperties {

	public string unitName = "unnamed";
	public float skillRanged = 100f;
	public float skillMelee	 = 100f;
	public int maxHealth = 8;
	public int armor = 0;
	
	public float size = 1.0f;

	internal Weapon weaponPrimary;
	internal Weapon weaponSecondary;

	public override string ToString() {
		return
			" Name=" + unitName +
			"\n Accuracy=" + skillRanged +
			"\n MaxHealth=" + maxHealth +
			"\n Armor=" + armor +
			"\n- - -";
	}

}

[System.Serializable]
public class UnitEquipment {

	public Weapon weaponPrimary;
	public Weapon weaponSecondary;

	public Biomod biomod;

	public Equippable[] misc = { };

	public UnitEquipment( Weapon weaponPrimary, Weapon weaponSecondary, Equippable[] misc, Biomod biomod = null ) {
		this.weaponPrimary = weaponPrimary;
		this.weaponSecondary = weaponSecondary;
		this.misc = misc;
		this.biomod = biomod;
	}

	public List<Equippable> Everything() {
		List<Equippable> list = new List<Equippable>();
		list.Add( weaponPrimary );
		list.Add( weaponSecondary );
		list.Add( biomod );
		list.AddRange( misc );
		return list;
	}

	internal UnitEquipment instance {
		get {
			List<Equippable> newMisc = new List<Equippable>();
			foreach( Equippable e in misc ) 
				newMisc.Add( GameObject.Instantiate( e ) as Equippable );
			return new UnitEquipment(
				GameObject.Instantiate( weaponPrimary ) as Weapon,
				GameObject.Instantiate( weaponSecondary ) as Weapon,
				newMisc.ToArray() , 
				biomod == null ? null : GameObject.Instantiate( biomod ) as Biomod
			);
		}
	}

}

public static class IsUnit {

	public static bool OutOfAmmo( Unit unit ) {
		return unit.currentWeapon.ranged && !( unit.currentWeapon as Firearm ).haveAmmo;
	}

}