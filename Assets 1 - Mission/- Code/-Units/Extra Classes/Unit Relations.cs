using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	public class UnitUnitRelations {

		private readonly Dictionary<Unit,UnitUnitRelation> relations;
		private readonly Unit owner;

		public Unit primaryEnemy;

		internal UnitUnitRelations( Unit ownUnit  ) {
			relations = new Dictionary<Unit, UnitUnitRelation>();
			owner = ownUnit;
		}

		internal void Update() {
			relations.Clear();
			foreach( Unit unit in God.allUnits ) {
				relations.Add( unit, new UnitUnitRelation( owner, unit ) );
			}
		}

		internal void Update( Unit subject ) {

			relations[subject] = new UnitUnitRelation( owner, subject );

		}

		internal UnitUnitRelation GetRelation( Unit unit ) {
			return relations[unit];
		}

		internal bool CanAttack( Unit unit ) {
		//	return true;
			return IsVisible( unit ) && GetAttackResult( unit ).hitChance > 0f;
		}

		internal bool IsVisible( Unit unit ) {
			return relations[unit].visible;
		}

		internal float GetDistance( Unit unit ) {
			return relations[unit].distance;
		}

		internal float GetAngle( Unit unit ) {
			return relations[unit].angle;
		}

		internal AttackResult GetAttackResult( Unit unit ) {
			return relations[unit].attackResult;
		}

		internal int CompareDistances( Unit u1, Unit u2 ) {
			return GetDistance( u1 ).CompareTo( GetDistance( u2 ) );
		}

		internal int CompareHitChances( Unit u1, Unit u2 ) {
			return GetAttackResult( u2 ).hitChance.CompareTo( GetAttackResult( u1 ).hitChance );
		}

		//////

		public static void recalculateRelations( Unit unit ) {

			foreach( Unit owner in God.allUnits ) {

				owner.relations.Update( unit );

			}

		}

	}

	public struct UnitUnitRelation {

		internal readonly bool visible;
		internal readonly float angle;
		internal readonly float distance;
		internal readonly float damageMax;
		internal readonly float critChance;
		internal readonly float critMultiplier;
		internal readonly AttackResult attackResult;

		public UnitUnitRelation( Unit owner, Unit unit ) {
			this.visible = owner.CanSee( unit.currentTile );
			this.distance = owner.GetDistance( unit.transform );
			this.damageMax = owner.propAttackDamage;
			this.critChance = 50;
			this.critMultiplier = 2;
			this.angle = Angles.FixAngleDegSigned( owner.currentTile.relations.GetAngle( unit.currentTile ) + owner.rotationY - 90 );
			this.attackResult = GetAttackResult( owner, unit );
		}

		private static AttackResult GetAttackResult( Unit attacker, Unit attackee ) {
			if( attacker.currentWeapon.ranged ) {
				return new RangedAttackResult( attacker, attackee );
			} else {
				return new MeleeAttackResult( attacker, attackee );
			}
		}

	}

	public class UnitObjectsInRange {

		internal List<Unit> units;
		internal List<Unit> allies;
		internal List<Unit> enemies;

		internal void Update( Unit owner, List<Unit> allUnits ) {

			UnitUnitRelations rel = owner.relations;

			units = allUnits.FindAll( ( u ) => ( owner.CanTarget( u ) ) );

			units.Sort( delegate( Unit u1, Unit u2 ) {
				return rel.CompareHitChances( u1, u2 );
			} );

			allies = units.FindAll( ( u ) => !owner.team.IsEnemy( u ) );
			enemies = units.FindAll( ( u ) => owner.team.IsEnemy( u ) );

		}

		internal bool HaveUnits() {
			return units.Count > 0;
		}

		internal bool HaveAllies( bool inclMe = false ) {
			return inclMe || ( allies.Count > 0 );
		}

		internal bool HaveEnemies() {
			return enemies.Count > 0;
		}

		internal void Clear() {
			units = new List<Unit>();
			allies = new List<Unit>();
			enemies = new List<Unit>();
		}

	}
