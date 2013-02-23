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

	//

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


	public class UnitStrike : UnitProcess {

		protected readonly Unit attacker;
		protected readonly Unit attackee;
		protected readonly MeleeWeapon weapon;

		protected readonly AttackResult result;

		public UnitStrike( Unit attacker, Unit attackee, MeleeWeapon weapon )
			: base( "Strike", attacker ) {

			this.attacker = attacker;
			this.attackee = attackee;
			this.weapon = weapon;

			result = attacker.relations.GetAttackResult( attackee );

		}

		protected override void _Start() {

			Debug.Log( attacker + " strikes " + attackee + ". Hit chance: " + result.hitChance );

			if( !attacker.canAttack ) {
				Debug.Log( "Attack failed because " + attacker + " cannot attack. " );
				End();
				return;
			}

			bool successful = result.msg == AttackResult.Message.SUCCESS;

			attacker.model.Attack( attackee , successful );
			weapon.Attack( attackee , result.hittee );

			Process p;

			p = new WaitForSeconds( UnitAnimation.MELEE_HIT_DELAY );

			processManager.AddImmediately( p );

			if( result.hittee != null ) {
				p.eventEnded +=
					() => result.hittee.Damage( attacker.propAttackDamage , attacker.currentWeapon.damageType , attacker );
			}

			p.eventEnded += End;

			Logger.Respond( "Attack result: " + result );

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}

	public class UnitShoot : UnitProcess {

		protected readonly Unit attacker;
		protected readonly Unit attackee;
		protected readonly Firearm weapon;

		protected readonly AttackResult result;

		public UnitShoot( Unit attacker, Unit attackee, Firearm weapon )
			: base( "Shoot", attacker ) {

			this.attacker = attacker;
			this.attackee = attackee;
			this.weapon = weapon;

			result = attacker.relations.GetAttackResult( attackee );

		}

		protected override void _Start() {
			Debug.Log( attacker + " shoots " + attackee + ". Hit chance: " + result.hitChance );

			if( !attacker.canAttack ) {
				Debug.Log( "Attack failed because " + attacker + " cannot attack. " );
				End();
				return;
			}

			bool successful = result.msg == AttackResult.Message.SUCCESS;

			attacker.model.Attack( attackee , successful );
			weapon.Attack( attackee , result.hittee );

			Process p;

			p = new WaitForSeconds( UnitAnimation.RANGE_HIT_DELAY );

			processManager.AddImmediately( p );

			if( result.hittee != null ) {
				p.eventEnded +=
					() => result.hittee.Damage( attacker.propAttackDamage , attacker.currentWeapon.damageType , attacker );
			}

			p.eventEnded += End;

			Logger.Respond( "Attack result: " + result );

		}

		protected override void _End() {
			GameMode.cinematic = false;
		}

	}

	//

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

	public abstract class ParabolicMove : UnitProcess {

		protected readonly Transform subject;
		protected readonly Transform destination;

		protected readonly float height;
		protected readonly float speed;

		protected Vector3 positionEnd;
		protected Vector3 positionStart;
		protected float distance;

		protected float progress;

		private readonly float slomoAt;
		private bool slomoDone;

		protected ParabolicMove( string processName, Unit unit, Transform subject, Transform destination, float height = 5f, float speed = 3f )
			: base( processName, unit ) {

			this.subject = subject;
			this.destination = destination;

			this.height = height;
			this.speed = speed;

			slomoAt = rand;

		}

		protected override void _Start() {

			positionStart = subject.transform.position;
			positionEnd = destination.transform.position;

			distance = Vector3.Distance( positionStart, positionEnd );

		}

		protected override void _Update() {

			progress += speed * GodOfTime.deltaTime;

			if( !slomoDone && progress > slomoAt ) {
				processManager.AddImmediately( new BulletTime( .02f, .75f ) );
				slomoDone = true;
			}

			if( progress >= 1f ) {
				subject.transform.position = positionEnd;
				End();
				return;
			}

			subject.transform.position = CalculatePosition();

		}

		private Vector3 CalculatePosition() {

			return Vector3.MoveTowards( positionStart, positionEnd, progress * distance )
				+ Vector3.up * Mathf.Sin( progress * Mathf.PI ) * height;

		}
		
	} // TODO THIS SHOULD NOT BE A UNIT PROCESS

	public class UnitJump : ParabolicMove { // TODO make this invoke and track a new ParabolicMove process instead of extending one

		protected readonly Unit jumper;
		protected readonly GridTile destinationTile;

		public UnitJump( Unit jumper, GridTile destinationTile, float height = 5f, float speed = 3f )
			: base( "Jump", jumper, jumper.transform, destinationTile.transform, height, speed ) {

			this.jumper = jumper;
			this.destinationTile = destinationTile;

		}

		protected override void _End() {
			jumper.currentTile = destinationTile;
			jumper.OnTileReached();
			jumper.OnCurrentTileReached();
			GameMode.cinematic = false;
		}

	}




















}
