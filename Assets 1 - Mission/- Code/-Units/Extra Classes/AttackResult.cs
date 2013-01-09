using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	public abstract class AttackResult {

		public enum Message { SUCCESS, MISSED, EVADED, HIT_COVER }

		public float hitChance;

		public Message msg;
		public bool evaded;
		public IDamageable hittee = null;

		public string longDescription = "";
		
		protected static float ChanceFromAccuracy( Unit attacker ) {
			if( Config.OVERRIDE_HIT_CHANCE_ACCURACY ) {
				return 1.0f;
			}
			return attacker.propAccuracy / 100;
		}

		protected static float ChanceFromUnitSize( float attackeeSize ) {
			return Config.OVERRIDE_HIT_CHANCE_UNIT_SIZE ? 1f : ( 0.5f + attackeeSize / 2 );
		}

		protected static float ChanceFromUnitEvasion( Unit attackee ) {
			return Config.OVERRIDE_HIT_CHANCE_UNIT_EVASION ? 1f : ( 1f - attackee.propEvasion / 100 );
		}

		public override string ToString() {
			return msg.ToString();
		}

	}

	public class RangedAttackResult : AttackResult {

		protected const float POINT_BLANK_RADIUS = 3f; // 1.5f

		public RangedAttackResult( Unit attacker, Unit attackee ) { //TODO replace target with IDamegable

			float mul_DistanceAndAccuracy	= 1f;
			float mul_CoversAndTargetSize	= 1f;
			float mul_TargetEvasion			= 1f;

			//start with "perfect conditions" chance which is based solely on skill
			mul_DistanceAndAccuracy *= ChanceFromAccuracy( attacker );
			//apply distance penalty
			mul_DistanceAndAccuracy *= ChanceFromDistance( attacker.GetDistance( attackee.transform ), attacker.propAttackRange );

			//apply protection from cover
			//apply unit size multilpier so larger units are easier to hit even from distance or behind small cover.
			mul_CoversAndTargetSize *= ChanceFromCoverAndSize( attacker.currentTile, attackee.currentTile, attackee.props.size );

			//apply evasion chance
			mul_TargetEvasion *= ChanceFromUnitEvasion( attackee );

			//calculate total chance, which will be shown before attack for user consideration
			this.hitChance = 100f * mul_DistanceAndAccuracy * mul_CoversAndTargetSize * mul_TargetEvasion;

			longDescription =
				"mul_DistanceAndAccuracy =" +
				"\n ChanceFromAccuracy~" + ChanceFromAccuracy( attacker ) +
				"\n ChanceFromDistance!~" + ChanceFromDistance( attacker.GetDistance( attackee.transform ), attacker.propAttackRange ) +
				"\nmul_CoversAndTargetSize =" +
				"\n ChanceFromUnitSize~" + ChanceFromUnitSize( attackee.props.size ) +
				"\n ChanceFromCoverAndSize~" + ChanceFromCoverAndSize( attacker.currentTile, attackee.currentTile, attackee.props.size ) +
				"\nmul_TargetEvasion =" +
				"\n ChanceFromUnitEvasion~" + ChanceFromUnitEvasion( attackee );

			if( !God.Chance1( mul_DistanceAndAccuracy ) ) {

				this.msg = Message.MISSED;
				return;

			} else {

				if( !God.Chance1( mul_CoversAndTargetSize ) ) {

					this.msg = Message.HIT_COVER;

					ICover[] covers = attackee.currentTile.relations.GetCoversAgainst( attacker.currentTile );
					if( covers.Length > 0 ) {
						this.hittee = covers[0] as IDamageable;
					}
					return;

				} else {

					if( !God.Chance1( mul_TargetEvasion ) ) {

						this.msg = Message.EVADED;
						this.evaded = true;
						return;

					} else {

						this.msg = Message.SUCCESS;
						this.hittee = attackee;
						return;

					}

				}

			}

		}

		protected static float ChanceFromDistance( float distance, float range ) {
			if( Config.OVERRIDE_HIT_CHANCE_DISTANCE ) {
				return 1.0f;
			}
			if( distance > range ) {
				return 0.0f;
			}
			if( distance < POINT_BLANK_RADIUS ) {
				return 1.0f;
			}
			//Decrement with POINT_BLANK_RADIUS so that we calculate from there outwards
			distance -= POINT_BLANK_RADIUS;
			range -= POINT_BLANK_RADIUS;
			float r = ( 2.0f - ( distance / range ) ) / 2.0f;
			return M.ClipMaxMin( r );
		}

		protected static float ChanceFromCoverAndSize( GridTile attackerTile, GridTile attackeeTile, float attackeeSize ) {
			if( Config.OVERRIDE_HIT_CHANCE_COVER ) {
				return 1.0f;
			}
			float r = ( ChanceFromUnitSize( attackeeSize ) - attackeeTile.relations.GetTotalCoverValueAgainst( attackerTile ) );
			return M.ClipMaxMin( r );
		}

	}

	public class MeleeAttackResult : AttackResult {

		public MeleeAttackResult( Unit attacker, Unit attackee ) {

			float mul_DistanceAndEfficiency	= 1f;
			float mul_TargetEvasion			= 1f;

			//start with "perfect conditions" chance which is based solely on skill
			mul_DistanceAndEfficiency *= ChanceFromAccuracy( attacker );
			//apply distance penalty
			mul_DistanceAndEfficiency *= ChanceFromDistance( attacker.GetDistance( attackee.transform ) );
			//apply unit size multilpier so larger units are easier to hit
			mul_DistanceAndEfficiency *= ChanceFromUnitSize( attackee.props.size );

			//apply evasion chance
			mul_TargetEvasion *= ChanceFromUnitEvasion( attackee );
			//apply weapon speed, so faster weapons are harder to evade
			mul_TargetEvasion *= ChanceFromSpeed( attacker.currentWeapon as MeleeWeapon );

			//calculate total chance, which will be shown before attack for user consideration
			this.hitChance = 100f * mul_DistanceAndEfficiency * mul_TargetEvasion;

			if( !God.Chance1( mul_DistanceAndEfficiency ) ) {

				this.msg = Message.MISSED;
				this.hittee = null;
				return;

			} else {

				if( !God.Chance1( mul_TargetEvasion ) ) {

					this.msg = Message.EVADED;
					this.hittee = null;
					this.evaded = true;
					return;

				} else {

					this.msg = Message.SUCCESS;
					this.hittee = attackee;
					return;

				}

			}

		}

		internal static float ChanceFromSpeed( MeleeWeapon weapon ) {

			return M.ClipMaxMin( weapon.speed / 100 ); //TODO add here units'sb chance to dodge

		}
		private static float ChanceFromDistance( float distance ) {

			return ( distance <= 1.5f ) ? 1f : 0f;

		}

	}