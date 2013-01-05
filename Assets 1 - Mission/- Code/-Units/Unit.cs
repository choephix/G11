using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MissionBaseClass, IDamageable, ICover, ISomethingOnGridTile {
	
	internal UnitModel model;

	public UnitTransformSpots spots;
	public TextMesh label;
	public UnitBillboard billboard;

	public UnitProperties props;
	public UnitStatus status;
	
	private GridTile _currentTile;
	public GridTile currentTile {
      	get{ return _currentTile; }
      	set{

			if( _currentTile ) {
				_currentTile.currentUnit = null;
				_currentTile.UpdateMaterial();
			}
			_currentTile = value;
			_currentTile.currentUnit = this;

			OnCurrentTileChanged();

		}
	}

	internal Weapon currentWeapon;
	internal Weapon weaponPrimary;
	internal Weapon weaponSecondary;
	
	internal Team team;
	internal int squad = 0;

	internal Actions actions = new Actions();
	internal UnitBuffs buffs = new UnitBuffs();
	internal UnitObjectsInRange objectsInRange = new UnitObjectsInRange();
	internal UnitUnitRelations relations = new UnitUnitRelations();

	//

	private bool ready = false;
	internal bool atCurrentTile = false;
	internal bool acting = false;
	internal float rotationY = 0;

	public bool selected { get { return ( selectedUnit == this ); } }
	public bool targeted { get { return ( targetedUnit == this ); } }
	public bool targeting { get { return selected && GameMode.Is(GameModes.PickUnit); } }

	internal bool alive = true;
	internal bool activated = false;
	internal bool concious { get { return ( alive ); } }
	internal bool inPlay { get { return ready && alive && activated && !currentTile.fogged; } }
	internal bool selectable { get { return ( canAct && atCurrentTile && team.isTheirTurn ); } }
	internal bool targetable { get { return ( inPlay ); } }
	internal bool canAct { get { return inPlay && hasActions; } }
	//internal bool canAct { get { return inPlay && hasActions && actions.count > 0; } }
	internal bool canMove { get { return canAct; } }
	internal bool canAttack { get { return canAct && objectsInRange.HaveEnemies() && currentWeapon.canAttack; } }
	internal bool hasActions { get { return ( status.actionPoints > 0 ); } }

	// PROPERTY GETTERS


	public float propAccuracy { get { return currentWeapon.ranged ? props.skillRanged : props.skillMelee; } }
	public float propAttackDamage { get { return currentWeapon.damage; } }
	public float propAttackRange { get { return currentWeapon.range; } }
	public int propMovementRange { get { return Config.DEV_UNIT_MOVE_RANGE; } }
	public int propSightRange { get { return Config.DEV_UNIT_SIGHT_RANGE; } }

	public float chanceToBeHit { get { return buffs[ BuffPropFlag.Defending ] ? .5f : 1f; } }

	public float propHealth { get { return status.health; } }
	public float coverValue { get { return 0.5f * props.size; } }

	// VISUAL BS

	/// <summary>
	/// Relative to the vurrent heading, on which side is the closest enemy. 
	/// (for choosing the right model animation and the right shoulder to put the camera on when targeting)
	/// false = LEFT; true = RIGHT;
	/// </summary>
	public bool actionSide {
		get { return model.actionSide; }
		set { model.actionSide = value; }
	}
	public float movementSpeed { get { return propMovementRange * .65f; } }

	// EVENTS

	internal event EventHandler eventSelected = delegate { };
	internal event EventHandler eventDeselected = delegate { };
	internal event EventHandler<Action> eventActionStarted = delegate { };
	internal event EventHandler<Action> eventActionFinished = delegate { };
	internal event UnitExtraEventHandler<Weapon> eventWeaponUnequipped = delegate { };
	internal event UnitExtraEventHandler<Weapon> eventWeaponEquipped = delegate { };
	internal event UnitEventHandler eventTileReached = delegate { };
	internal event UnitEventHandler eventCurrentTileReached = delegate { };
	internal event UnitUnitEventHandler eventDeath = delegate { };

	// DEVELOPMENT

	public bool focused;
	
	/** * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/

	void Awake() {

		actions.Init( this );

		Action action;
		action = new ActionsBook.Move( this, this );
		actions += action;
		actions.defaultAction = action;

		action = new ActionsBook.Defend( this, this );
		actions += action;

		action = new ActionsBook.Test( this );
		actions += action;

	}

	void Start () {

		name = props.unitName;
		status.Init( props );

		weaponPrimary = model.InstantiateWeapon( weaponPrimary );
		weaponPrimary.Init( this );
		model.HideWeapon( weaponPrimary );
		weaponSecondary = model.InstantiateWeapon( weaponSecondary );
		weaponSecondary.Init( this );
		model.HideWeapon( weaponSecondary );

		EquipWeapon( weaponPrimary );

		if( !team.isUserControlled ) { //TODO this should so not be here
			model.meshRenderer.renderer.enabled = false;
			model.HideWeapon( currentWeapon );
		}

		__SetFlag( false );

	}

	void Update () {

		if( model && alive ) {

			if( currentTile != null && !atCurrentTile && activated && processQueue.empty ) {
				transform.position = currentTile.transform.position;
				OnCurrentTileReached();
			}

		} else {

			if( !alive ) {

				Vector3 spatula = ( currentTile.transform.position - transform.position ) * .1f;
				transform.position += spatula;

			}

		}

		tempUpdate();

	}


	private float currentTileDistance;
	private float stepLength;
	private void UpdateMovement() {

		currentTileDistance = Vector3.Distance( transform.position, currentTile.transform.position );
		stepLength = GodOfTime.deltaTime * movementSpeed;

		model.LoopClip( currentTile.obstructed ? UnitAnimation.JUMP : UnitAnimation.MOVE, .2f );

		if( currentTileDistance < stepLength ) {

			OnCurrentTileReached();

		} else {

			transform.LookAt( currentTile.transform.position );
			transform.position = Vector3.MoveTowards( transform.position, currentTile.transform.position, stepLength );

		}

	}

	private void tempUpdate() { //TODO always check this for something to remove
		
		bool showBillboard = false;

		if( inPlay ) {

			billboard.UpdateLabels( this );

			if( GameMode.interactive && TurnManager.isUserTurn ) {
				if( GameMode.selecting ) {
					showBillboard = TurnManager.isUserTurn && ( selected || currentTile.focused );
				}
				if( GameMode.targeting ) {
					showBillboard = targeted;
				}
			}

		}

		billboard.visible = showBillboard;


	}

	/** ACTIONS * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/


	internal void OnAction( Action action ) {

		acting = true;

		status.actionPoints -= action.cost;

		if( ready ) {
			GameMode.Disable();
		}

	}

	internal void OnActionFinished( Action action ) {

		acting = false;

		eventActionFinished.Invoke( action );

		UpdateEverything();

	}


	private void OnCurrentTileChanged() {

		atCurrentTile = false;

	}

	public void OnCurrentTileReached() {
		if( !atCurrentTile ) {

			atCurrentTile = true;
			_currentTile.UpdateMaterial();

			transform.position = currentTile.transform.position;

			model.currentSmallestCover = null;
			foreach( GridTile cover in _currentTile.relations.neighbours ) {
				if( cover.obstructed && !_currentTile.relations.relations[cover].diagonal ) {
					if( model.currentSmallestCover == null || model.currentSmallestCover.coverValue > cover.coverValue ) {
						model.currentSmallestCover = cover;
					}
				}
			}

			model.UpdatePosture();

			rotationY = transform.eulerAngles.y;
			model.LoopIdleClip();

			if( !ready ) {
				ready = true;
				God.OnReady_Unit( this ); //TODO delegate
				if( activated ) {
					model.ShowWeapon( currentWeapon );
				}
			}

			eventCurrentTileReached.Invoke(this);

		}
	}

	public void OnTileReached() {

		eventTileReached.Invoke( this );

	}

	public void Damage( float amount, DamageType type, Unit attacker = null ) {

		status.ReceiveDamage( amount, type );

		if( status.health <= 0 ) {
			Die( attacker );
		} else {
			model.Damage();
		}

	}

	public void Die( Unit killer ) {

		alive = false;
		currentTile.currentUnit = null;
		collider.enabled = false;
		model.Die();
		eventDeath.Invoke( this, killer );

		if( killer ) {
			transform.LookAt( killer.transform );
		}

	}

	public void SwitchWeapon() {

		EquipWeapon( currentWeapon == weaponPrimary ? weaponSecondary : weaponPrimary );
		if( targeting ) {
			model.Aim();
		}

		if( ready ) {
			UpdateEverything();
		}

	}

	public void EquipWeapon( Weapon weapon ) {

		Debug.Log( this + " equipping new weapon " + weapon + ", instead of " + F.ToStringOrNull( currentWeapon ) );

		if( currentWeapon ) {
			model.HideWeapon( currentWeapon );
			eventWeaponUnequipped.Invoke( this, currentWeapon );
		}

		model.currentWeapon =
		currentWeapon = weapon;

		//if( currentWeapon is Firearm ) {
		//    ( currentWeapon as Firearm ).eventReload += NewReload;
		//}

		eventWeaponEquipped.Invoke( this, currentWeapon );
		model.ShowWeapon( currentWeapon );

		model.Reload();

	}

	/** * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/

	internal float GetDistance( Transform o ) {
		return Vector3.Distance( transform.position, o.position );
	}
	internal bool IsObjectInRange( GameObject o ) {
		return GetDistance( o.transform ) <= ( propSightRange );
	}
	internal bool CanSee( GridTile tile ) {
		return GodOfPathfinding.GetLine( currentTile, tile ).FindAll( t => t.coverValue >= 1f ).Count == 0;
	}

	internal float CalculateHitChance( Unit target ) {

		//start with "perfect conditions" chance which is based solely on skill
		float chance = Config.OVERRIDE_HIT_CHANCE_ACCURACY ? 100 : propAccuracy;

		if( currentWeapon.ranged ) {

			//apply distance a distance penalty
			chance *= UnitMath.GetHitChancePenalty_Distance( GetDistance( target.transform ), propAttackRange );

			//apply protection from cover
			chance *= UnitMath.GetHitChancePenalty_Cover( currentTile, target.currentTile );

			//apply unit size multilpier so larger units are easier to hit even from distance or behind small cover.
			chance *= UnitMath.GetHitChancePenalty_UnitSize( target );

			chance *= target.chanceToBeHit;

			chance *= Config.DEV_HIT_CHANCE_MULTIPLIER;

		} else {

			chance *= UnitMath.GetHitChancePenalty_MeleeWeapon( currentWeapon as MeleeWeapon, GetDistance( target.transform ) ); //TODO add here units'sb chance to dodge

		}

		return M.ClipMaxMin( chance, 100.0f );

	}
	internal bool CanTarget(Unit unit) {
		return
			unit != this &&
		//	!unit.selected &&
			unit.targetable &&
			relations.IsVisible( unit ) &&
			relations.GetHitChance( unit ) > 0 &&
			CanWeaponTarget(unit)
			;
	}
	internal bool CanWeaponTarget( Unit unit ) {

	//	return true;

		if( currentWeapon.targetType == TargetType.Enemy && !team.IsEnemy( unit ) ) return false;
		if( currentWeapon.targetType == TargetType.Ally && !team.IsAlly( unit ) ) return false;

		return currentWeapon.CanTarget( unit );

	}

	/** * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/

	internal void UpdateEverything() {

		relations.Update( this, allUnits );
		objectsInRange.Update( this, allUnits );

		if( !activated && objectsInRange.enemies.Count > 0 ) {
			team.ActivateSquad( squad );
		}

		if( inPlay ) {

			relations.primaryEnemy = objectsInRange.HaveEnemies() ? objectsInRange.enemies[0] : null;

			if( selected && targetedUnit ) {
				model.actionSide =
				actionSide = ( relations.GetAngle( targetedUnit ) ) <= 0;
			//	Debug.Log( this + "'sb actionSide has been set to " + ( actionSide ? "RIGHT" : "LEFT" ) + " because targetedUnit is at " + relations.GetAngle( targetedUnit ) );
				if( !CanTarget( targetedUnit ) ) {
					GameMode.Reset();
				}
			} else
				if( relations.primaryEnemy ) {
					model.actionSide =
					actionSide = ( relations.GetAngle( relations.primaryEnemy ) ) <= 0;
					if( selected ) {
					//	Debug.Log( this + "'sb actionSide has been set to " + ( actionSide ? "RIGHT" : "LEFT" ) + " because primaryEnemy is at " + relations.GetAngle( relations.primaryEnemy ) );
					}
				}

			model.RefreshPosture();

			model.adjescentEnemy = relations.primaryEnemy && ( relations.GetDistance( relations.primaryEnemy ) < 1.5f );

			ClearFog( currentTile );

		}

	}

	internal void Activate() {
		activated = true;
		model.meshRenderer.renderer.enabled = true;
		if( currentWeapon ) {
			model.ShowWeapon( currentWeapon );
		}
		model.Reload();
		Debug.Log( this + " activated!" );
	}

	public void ClearFog( GridTile centerTile ) {

		if( team.isUserControlled && Config.USE_FOG ) {
			foreach( GridTile tile in grid.GetAllTiles() ) {
				if( ( centerTile.relations.GetDistance( tile ) < propAttackRange || GetDistance(tile.transform) < propSightRange ) && CanSee(tile) ) {
					tile.UnFog();
				}
			}
		}

	}
	
	public override string ToString() {
		return "<"+this.props.unitName+">";
	}



	internal void OnOurTurnStart() {
		if( inPlay ) {
			UpdateEverything();
			buffs.OnTurnStart();
			status.ResetActions();
			actions.OnTurnStart();
		}
	}

	internal void OnSelected() {

		eventSelected.Invoke();

		UpdateEverything();

	}

	internal void OnDeselected() {

		eventDeselected.Invoke();

		model.RefreshPosture();

	}

	internal void OnTargetingUnit( Unit targetedUnit ) {

		UpdateEverything();
		transform.LookAt( targetedUnit.transform );
		model.Aim();

	}

	internal void OnUntargetingUnit() {
		model.LoopIdleClip();
		transform.eulerAngles = Vector3.up * rotationY;
	}

	internal void OnTargetedBy( Unit targetingUnit ) {
		if( model.posture == UnitModelPosture.Normal ) {
			transform.LookAt( targetingUnit.transform );
		}
	}

	void OnMouseExit() {
		currentTile.OnMouseExit(); //TODO refactor out Hover() OUt() functions called by OnMouse...()
	}

	void OnMouseEnter() {
		currentTile.OnMouseEnter();
	}

	void OnMouseUp() {
		ClickHandler.Up( this );
	}

	internal void __SetFlag( bool up ) {
		transform.Find( "flag" ).gameObject.SetActiveRecursively( up );
	}

	internal void SetModel( UnitModel unitModel ) {
		model = Instantiate( unitModel, transform.position, transform.rotation ) as UnitModel;
		model.transform.parent = transform;
		model.Init( this );
	}

	internal void SetMaterial( Material material ) {
		model.SetMaterial( material );
	}

}