using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public partial class ProcessBook {

	// UNIT PROCESSES

	public abstract class UnitProcess : Process {

		public readonly Unit subjectUnit;
		protected readonly bool evenInDeath;

		protected UnitProcess( string name, Unit subject, bool evenInDeath = false )
			: base( name ) {
			this.evenInDeath = evenInDeath;
			subjectUnit = subject;
		}

		public sealed override void Update() {

			if( subjectUnit.concious || evenInDeath ) {

				base.Update();

			} else {

				Debug.Log("Unit process " + name + " did not finish because "
				                          + subjectUnit + " is dead or unconcious" );

				End();

			}

		}

	}


	public class UnitAttack : UnitProcess {

		protected readonly Unit attacker;
		protected readonly Unit attackee;

		protected readonly AttackResult result;
		protected readonly Weapon weapon;

		public UnitAttack( Unit attacker, Unit attackee )
			: base( "Attack", attacker ) {

			this.attacker = attacker;
			this.attackee = attackee;

			result = attacker.relations.GetAttackResult( attackee );
			weapon = attacker.currentWeapon;

		}

		protected override void _Start() {

			Debug.Log( attacker + " attacks " + attackee + ". Hit chance: " + result.hitChance );

			if( attacker.canAttack ) {

				//GameMode.cinematic = true;

				bool successful = result.msg == AttackResult.Message.SUCCESS;

				attacker.model.Attack( attackee, successful );
				weapon.Attack( attackee, result.hittee );

				Process p;

				p = new WaitForSeconds( weapon.ranged ? UnitAnimation.RANGE_HIT_DELAY : UnitAnimation.MELEE_HIT_DELAY );

				//Attach( p );
				processManager.AddImmediately( p );

				if( result.hittee != null ) {
					p.eventEnded +=
						() => result.hittee.Damage( attacker.propAttackDamage, attacker.currentWeapon.damageType, attacker );
				}

				p.eventEnded += End;

				Logger.Respond( "Attack result: " + result );

			} else {

				Debug.Log( "Attack failed because " + attacker + " cannot attack. " );
				End();

			}

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}

	public class UnitHeal : UnitProcess {

		public const float HEALING_SPEED = .25f;

		public readonly float healingAmount;
		public float healingProgress;

		public UnitHeal( Unit subject, float amount )
			: base( "Healing", subject ) {
			healingAmount = amount;
		}

		protected override void _Update() {

			if( healingProgress < healingAmount ) {

				healingProgress += HEALING_SPEED;
				subjectUnit.propHealth += HEALING_SPEED;

			} else {

				End();

			}

		}

	}

	public class Throw : UnitProcess {

		protected const float SPEED		= .045f;
		protected const float HEIGHT	= 1.5f;

		protected readonly Unit thrower;
		protected readonly Transform throwee;

		protected Vector3 positionEnd;
		protected Vector3 positionStart;
		protected float distance;

		protected float progress;

		public Throw( Unit thrower, Transform throwee, Vector3 destination, float inaccuracy = 0f )
			: base( "Throw", thrower ) {

			this.thrower = thrower;
			this.throwee = throwee;

			positionEnd = destination;

			if( !inaccuracy.NotZero() ) return;

			positionEnd += Vector3.forward * Random.Range( -inaccuracy, inaccuracy );
			positionEnd += Vector3.left * Random.Range( -inaccuracy, inaccuracy );

		}

		protected override void _Start() {
			thrower.model.Reload();
			positionStart = throwee.transform.position;
			distance = Vector3.Distance( positionStart, positionEnd );
			throwee.transform.parent = null;
			throwee.transform.localPosition = Vector3.zero;
		}

		protected override void _Update() {

			progress += SPEED;

			throwee.position = CalculatePosition();

			if( progress >= 1f ) {
				End();
			}

		}

		private Vector3 CalculatePosition() {

			Vector3 r;

			r = Vector3.MoveTowards( positionStart, positionEnd, progress * distance );

			r += Vector3.up * Mathf.Sin( progress * Mathf.PI ) * HEIGHT;

			return r;

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}




















}
