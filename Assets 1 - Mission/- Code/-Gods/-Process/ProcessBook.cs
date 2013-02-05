using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public partial class ProcessBook : MissionBaseClass {

	// COMMON PROCESSES

	public abstract class SimpleProcess : Process {

		protected SimpleProcess( string name, bool stackable = true ) : base( name, stackable ) { }

		public sealed override void Update() { base.Update(); }

	}

	public class Nothing : SimpleProcess {

		public Nothing() : base( "Nothing" ) {}

		protected override void _Start() { End(); }

	}

	public class InstantProcess : SimpleProcess {

		public event EventHandler eventOnStart;

		public InstantProcess( EventHandler eventOnStart, string name = null ) : base( name ?? "InstantProcess" ) { this.eventOnStart = eventOnStart; }

		protected override void _Start() {

			eventOnStart.Invoke();

			End();

		}

	}

	public class Trace : InstantProcess {

		public string msg;

		public Trace( string msg ) : 
			base( () => Debug.Log( msg ) , "Trace" ) {}

	}









	public class Wait : SimpleProcess {

		protected int framesLeft;

		public Wait( int frames ) : base( "WaitForFrames" ) { framesLeft = frames; }

		protected override void _Update() {
			if( framesLeft <= 0 ) {
				End();
			} else {
				framesLeft--;
			}
		}

		public override string ToString() {

			return base.ToString() + " (" + framesLeft + ")";

		}

	}

	public class WaitForSeconds : SimpleProcess {

		protected float timeLeft;

		public WaitForSeconds( float seconds ) : base( "WaitForSeconds" ) { timeLeft = seconds; }

		protected override void _Update() {
			if( timeLeft <= 0f ) {
				End();
			} else {
				timeLeft -= GodOfTime.deltaTime;
			}
		}

		public override string ToString() {

			return base.ToString() + " (" + timeLeft.Round( 1 ) + ")";

		}

	}












	public class ChangeTimeSpeed : SimpleProcess {

		private readonly float newSpeed;
		private readonly float processDuration;

		private float stepPerSecond;
		private bool negative;
		private bool instant;

		public ChangeTimeSpeed( float newSpeed, float processDuration = 0f )
			: base( "ChangeTimeSpeed", false ) {

			this.newSpeed = newSpeed;
			this.processDuration = processDuration;

		}

		protected override void _Start() {

			if( processDuration > 0f ) {

				stepPerSecond = newSpeed - GodOfTime.speed;
				negative = stepPerSecond < 0;
				stepPerSecond /= processDuration;

			} else {

				instant = true;

			}

		}

		protected override void _Update() {

			if( instant ) {
				GodOfTime.speed = newSpeed;
				End();
			} else {

				if( Math.Abs( GodOfTime.speed - newSpeed ) < Mathf.Epsilon || negative == ( GodOfTime.speed < newSpeed ) ) {
					GodOfTime.speed = newSpeed;
					End();
				} else {
					GodOfTime.speed += Time.deltaTime * stepPerSecond;
				}
			}

		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name + " (" + GodOfTime.speed.Round( 2 ) + '/' + newSpeed + ')';

		}

	}

	public class BulletTime : SimpleProcess {

		private readonly bool instant;
		private readonly float tempSpeed;
		private readonly float duration;
		private readonly float fade;
		private float time;

		private float stepPerSecond;

		public BulletTime( float tempSpeed, float duration = 1f, float fade = 0f )
			: base( "BulletTime", false ) {

			this.tempSpeed = tempSpeed;
			this.duration = duration;
			this.fade = fade;
			time = 0;
			instant = fade <= 0;

		}

		protected override void _Start() {

			if( !instant ) {

				stepPerSecond = tempSpeed - GodOfTime.speed;
				stepPerSecond /= fade;

			}

		}

		protected override void _Update() {

			time += Time.deltaTime;

			if( instant ) {

				if( time < duration ) {
					GodOfTime.speed = tempSpeed;
				} else {
					End();
				}

			} else {

				if( time < fade ) {

					GodOfTime.speed += Time.deltaTime * stepPerSecond;

				} else if( time < duration - fade ) {

					GodOfTime.speed = tempSpeed;

				} else if( time < duration ) {

					GodOfTime.speed -= Time.deltaTime * stepPerSecond;

				} else {

					End();

				}

			}

		}

		protected override void _End() {
			GodOfTime.speed = 1f;
		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name + " (" + GodOfTime.speed.Round( 2 ) + '/' + tempSpeed + ')';

		}

	}
















	public class AreaDamage : SimpleProcess {

		private Vector3 center;
		private readonly float range;
		private readonly float damageAmount;
		private readonly DamageType damageType;

		public AreaDamage( Vector3 center, float range, float damageAmount, DamageType damageType = DamageType.NORMAL )
			: base( "AreaDamage" ) {

			this.center = center;
			this.range = range;
			this.damageAmount = damageAmount;
			this.damageType = damageType;

		}

		protected override void _Start() {

			foreach( Unit unit in allUnits ) {

				if( Vector3.Distance( center, unit.transform.position ) < range ) {
					unit.Damage( damageAmount, damageType );
					unit.transform.LookAt( center.Flatten() );
				}

			}

			TempObject splosion = Instantiate(
				BookOfEverything.me.gfx[0], center, Quaternion.identity ) as TempObject;
			if( splosion != null ) 
				splosion.eventDeath += End;

			Instantiate(
				BookOfEverything.me.gfx[1], center, Quaternion.identity );

		}

		public override string ToString() {

			if( !started ) {
				return name;
			}

			return name;

		}

	}










}
