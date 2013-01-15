using UnityEngine;
using System.Collections;










public enum GameModes : byte { Disabled, Normal, PickTile, PickUnit, GameOver, Default }

public enum DamageType : byte { NORMAL, INTERNAL, ANTIARMOR, HEALING, TRANQUILIZER } //normal, biological, armor-piercing

internal enum Clip : byte { Idle, Run, Shoot, Die }

public enum UnitModelPosture { Normal, CoverWall, CoverDucked }

public enum EquipmentType : byte { SideArm, Rifle, Sword, Claws, Misc, Hidden }

public enum TargetType : byte { Any, Enemy, Ally }

public enum AmmoType : byte { BulletSmall, BulletLarge, Rocket }

//public enum ItemType : byte { undefined, rangedOneHanded, rangedTwoHanded, rangedHeavy, meleeFist, meleeWielded, throwable }

public enum ActionSubjectType { Self, GridTile, Unit }


public enum BuffTerminationCondition { Eternal, Timeout, NextAction }

public enum BuffPropFlag { 
	CantMove,
	CantShoot,
}
public enum BuffPropMult { 
	RangedChance, MeleeChance,
	RangedDamage, MeleeDamage, 
	Defence, Evasion,
	Height
}