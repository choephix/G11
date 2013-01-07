using UnityEngine;
using System.Collections;
using System.Collections.Generic;


	//TODO WorldObject public new Renderer renderer;


public class Equippable : MissionBaseClass{

	public new string name = "Unnamed";

	public MeshRenderer model;
	public TargetType targetType = TargetType.Enemy;
	private bool passive { get { return actions.Count == 0; } }

	public List<Action> actions = new List<Action>();

	protected Unit owner;

	public virtual void Init( Unit owner ) {
		this.owner = owner;
		gameObject.AddComponent<AnimationSpeedFixer>();
	}

}


public abstract class Weapon : Equippable {

	// PROPERTIES

	public ItemType type = ItemType.undefined;

	public float baseDamage = 10;
	public float baseRange = 1.5f;

	// PROPERTY GETTERS

	internal bool ranged { get { return this is Firearm; } }
	internal bool melee { get { return this is MeleeWeapon; } }
	internal float range { get { return baseRange; } }
	internal float damage { get { return baseDamage; } }
	internal DamageType damageType { get { return DamageType.NORMAL; } }
	internal bool canAttack { get { return true; } }

	// PRIVATES

	internal EventHandler<Weapon> eventAttackFinished = delegate { };

	public override void Init( Unit owner ) {
		base.Init( owner );
		actions.Add( new ActionsBook.Attack(owner, this) );
	}

    public abstract void Attack( Unit targetUnit, IDamageable hittee );

	public abstract bool CanTarget( Unit targetUnit );

	public override string ToString() {
		//	return name + "(" + type + ")";
		return name;
	}

}
