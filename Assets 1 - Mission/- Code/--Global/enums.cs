using UnityEngine;
using System.Collections;










public enum GameModes : byte { Disabled, Normal, PickTile, PickUnit, GameOver, Default }

public enum DamageType : byte { NORMAL, INTERNAL, ANTIARMOR, HEALING, TRANQUILIZER } //normal, biological, armor-piercing

internal enum Clip : byte { Idle, Run, Shoot, Die }

public enum UnitModelPosture { Normal, CoverWall, CoverDucked }

public enum equipmentSpot : byte { none, beltRight, beltLeft, backRight, backLeft, head }

public enum TargetType : byte { Any, Enemy, Ally }

public enum AmmoType : byte { bulletSmall, bulletLarge, rocket }

public enum ItemType : byte { undefined, rangedOneHanded, rangedTwoHanded, rangedHeavy, meleeFist, meleeWielded, throwable }

public enum ActionSubjectType { Self, GridTile, Unit }



public enum BuffPropFlag { 
	CantMove,
	CantShoot,
}
public enum BuffPropMult { 
	RangedChance, MeleeChance,
	RangedDamage, MeleeDamage, 
	Defence, Evasion
}