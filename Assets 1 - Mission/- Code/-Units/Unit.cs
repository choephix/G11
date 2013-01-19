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
	public UnitEquipment equipment;

	private GridTile _currentTile;
	public GridTile currentTile {
		get { return _currentTile; }
		set {

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

	internal Team team;
	internal int squad = 0;

	internal Actions actions = new Actions();
	internal UnitBuffs buffs = new UnitBuffs();
	internal readonly UnitObjectsInRange objectsInRange = new UnitObjectsInRange();
	internal readonly UnitUnitRelations relations = new UnitUnitRelations();

	//

	private bool ready;
	internal bool atCurrentTile;
	internal float rotationY;

	public bool selected { get { return ( selectedUnit == this ); } }
	public bool targeted { get { return ( targetedUnit == this ); } }
	public bool targeting { get { return selected && GameMode.Is( GameModes.PickUnit ); } }

	internal bool alive = true;
	internal bool activated = false;
	internal bool concious { get { return ( alive ); } }
	internal bool inPlay { get { return ready && concious && activated; } }
	internal bool selectable { get { return ( canAct && atCurrentTile && team.isTheirTurn ); } }
	internal bool targetable { get { return ( inPlay ); } }
	internal bool canAct { get { return inPlay && hasActions; } }
	//internal bool canAct { get { return inPlay && hasActions && actions.count > 0; } }
	internal bool canMove { get { return canAct; } }
	//internal bool canAttack { get { return canAct && objectsInRange.HaveEnemies() && currentWeapon.canAttack; } }
	internal bool canAttack { get { return currentWeapon.canAttack && !buffs[BuffPropFlag.CantShoot]; } } //TODO move enemiesInRange check elsewhere
	//internal bool canAttack { get { return objectsInRange.HaveEnemies() && currentWeapon.canAttack && !buffs[BuffPropFlag.CantShoot]; } }
	internal bool hasActions { get { return ( status.actionPoints > 0 ); } }

	// PROPERTY GETTERS


	public float propAccuracy { get { return currentWeapon.ranged ? props.skillRanged : props.skillMelee; } }
	public float propAttackDamage { get { return currentWeapon.damage; } }
	public float propAttackRange { get { return currentWeapon.range; } }
	public int propMovementRange { get { return Config.DEV_UNIT_MOVE_RANGE; } }
	public int propSightRange { get { return Config.DEV_UNIT_SIGHT_RANGE; } }

	public float propHeight { get { return props.size * buffs[BuffPropMult.Height]; } }
	public float propEvasion { get { return 0 * buffs[BuffPropMult.Evasion]; } }

	public float propHealth { get { return status.health; } set { status.health = value; } }
	public float coverValue { get { return 0.5f * propHeight; } }

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
	//TODO this whole thing is a mess.

	// DEVELOPMENT

	public bool focused;

	[Multiline]
	public string buginfo = "";

	/** * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/

	void Awake() {

		actions.Init( this );

		Action action;
		action = new ActionsBook.Move( this, this );
		actions += action;
		actions.defaultAction = action;

		action = new ActionsBook.Crouch( this );
		actions += action;

		action = new ActionsBook.Defend( this, this );
		actions += action;

		action = new ActionsBook.StitchUp( this );
		actions += action;

		action = new ActionsBook.Test( this );
		actions += action;

	}

	void Start() {

		name = props.unitName;
		status.Init( props );

		Add( equipment.weaponPrimary );
		Add( equipment.weaponSecondary );

		if( equipment.biomod != null ) {
			Add( equipment.biomod );
		}

		foreach( Equippable tool in equipment.misc ) {
			if( tool != null ) {
				Add( tool );
			}
		}

		//foreach( Equippable tool in equipment.Everything() ) {
		//    if( tool != null ) {
		//        tool.Init( this );
		//        model.Add( tool );
		//        model.Hide( tool );
		//    }
		//}

		EquipWeapon( equipment.weaponPrimary );


		if( !team.isUserControlled ) { //TODO this should so not be here
			model.meshRenderer.renderer.enabled = false;
			model.Hide( currentWeapon );
		}

		__SetFlag( false );

	}

	void Update() {

		if( model && alive ) {

			if( currentTile != null && !atCurrentTile && activated && processManager.empty ) {
				transform.position = currentTile.transform.position;
				OnCurrentTileReached();
			}

		} else {

			if( !alive ) {

				Vector3 spatula = ( currentTile.transform.position - transform.position ) * .1f;
				transform.position += spatula;

			}

		}

		TempUpdate();

	}

	public void Add( Equippable item ) {
		item.Init( this );
		model.Equip( item );
		model.Hide( item );
	}

	public void Remove( Equippable item ) {
		//item.Init( this );
		//model.Add( item );
		//model.Hide( item );
	}

	private void TempUpdate() { //TODO always check this for something to remove

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

		if( showBillboard ) {

			if( selected ) {

				model.materialManager.SetMode( UnitMaterialMode.Selected );

			} else if( !selected && selectable ) {

				model.materialManager.SetMode( UnitMaterialMode.HoverSelectable );

			} else if( !targeted && targetable ) {

				model.materialManager.SetMode( UnitMaterialMode.HoverTargetable );

			} else {

				model.materialManager.SetMode( UnitMaterialMode.Normal );

			}

		} else {

			if( targeted ) {
				model.materialManager.SetMode( UnitMaterialMode.Targeted );
			} else if( selected ) {
				model.materialManager.SetMode( UnitMaterialMode.Selected );
			} else {
				model.materialManager.SetMode( UnitMaterialMode.Normal );
			}

		}


	}

	/** ACTIONS * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/


	internal void OnActionStart( Action action ) {

		Events.unitActionStarted( this , action );

		eventActionStarted.Invoke( action );

		status.actionPoints -= action.cost;

	}

	internal void OnActionFinished( Action action ) {

		Events.unitActionFinished( this, action );

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
					model.Show( currentWeapon );
				}
			}

			eventCurrentTileReached.Invoke( this );

		}
	}

	public void OnTileReached() {

		eventTileReached.Invoke( this );

	}

	public void Damage( float amount, DamageType type, Unit attacker = null ) {

		if( alive ) {

			status.ReceiveDamage( amount, type );

			if( attacker ) {
				transform.LookAt( attacker.transform );
			}

			if( propHealth <= 0 ) {
				Die( attacker );
			} else {
				model.Damage();
			}

		}

	}

	public void Die( Unit killer ) {

		if( !alive ) return;

		alive = false;
		currentTile.currentUnit = null;
		collider.enabled = false;
		model.Die();
		eventDeath.Invoke( this, killer );

		processManager.Add( new ProcessBook.BulletTime( .05f, .85f, .2f ), true );

		if( Config.GORE ) {
			TempObject bloodspatter = Instantiate(
			                                      BookOfEverything.me.gfx[2],
			                                      transform.position,
			                                      transform.rotation ) as TempObject;

			if( killer ) {
				if( bloodspatter != null )
					bloodspatter.transform.LookAt( killer.transform );
			}
		}

		model.BloodyUp();

		model.BloodyUp();

	}

	public void SwitchWeapon() {

		EquipWeapon( currentWeapon == equipment.weaponPrimary ? equipment.weaponSecondary : equipment.weaponPrimary );
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
			model.Hide( currentWeapon );
			eventWeaponUnequipped.Invoke( this, currentWeapon );
		}

		model.currentWeapon =
		currentWeapon = weapon;

		//if( currentWeapon is Firearm ) {
		//    ( currentWeapon as Firearm ).eventReload += NewReload;
		//}

		eventWeaponEquipped.Invoke( this, currentWeapon );
		model.Show( currentWeapon );

		model.Reload();

	}

	/** * * * * * * * * * * * * * * * * * * * * * * * * * * * * * **/

	internal float GetDistance( Transform o ) {
		return Vector3.Distance( transform.position, o.position );
	}
	internal bool IsInVisualRange( GameObject o ) {
		return GetDistance( o.transform ) <= ( propSightRange );
	}
	internal bool CanSee( GridTile tile ) {
		return IsInLineOfSight( tile ) && IsInVisualRange( tile.gameObject );
	}

	private bool IsInLineOfSight( GridTile tile ) {
		return GodOfPathfinding.GetLine( currentTile, tile ).FindAll( t => t.coverValue >= 1f ).Count == 0;
	}

	internal bool CanTarget( Unit unit ) {
		return
			unit != this &&
			//	!unit.selected &&
			unit.targetable &&
			relations.IsVisible( unit ) &&
			relations.CanAttack( unit ) &&
			CanWeaponTarget( unit )
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
			} else
				if( relations.primaryEnemy ) {
					model.actionSide =
					actionSide = ( relations.GetAngle( relations.primaryEnemy ) ) <= 0;
					if( selected ) {
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
			model.Show( currentWeapon );
		}
		model.Reload();
		status.ResetActions();

		model.materialManager.SetMode( UnitMaterialMode.Normal );

		Debug.Log( this + " activated!" );

	}

	public void ClearFog( GridTile centerTile ) {

		if( team.isUserControlled && Config.USE_FOG ) {
			foreach( GridTile tile in grid.GetAllTiles() ) {
				if( CanSee( tile ) ) {
					tile.UnFog();
				}
			}
		}

	}

	public override string ToString() {
		return "<" + props.unitName + ">";
	}



	internal void OnOurTurnStart() {

		if( inPlay ) {
			UpdateEverything();
			buffs.OnTurnStart();
			status.ResetActions();
			actions.OnTurnStart();
		}

		Events.unitTurnStarted( this );

		model.materialManager.SetMode( UnitMaterialMode.Normal );

	}

	internal void OnSelected() {

		eventSelected.Invoke();

		UpdateEverything();

		model.materialManager.SetMode( UnitMaterialMode.Selected );

	}

	internal void OnDeselected() {

		eventDeselected.Invoke();

		model.RefreshPosture();

		model.materialManager.SetMode( UnitMaterialMode.Normal );

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

		model.materialManager.SetMode( UnitMaterialMode.Targeted );

	}

	internal void OnUntargetedBy( Unit targetingUnit ) {

		model.materialManager.SetMode( UnitMaterialMode.Normal );

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
		transform.Find( "flag" ).gameObject.SetActive( up );
	}

	internal void SetModel( UnitModel unitModel ) {
		model = Instantiate( unitModel, transform.position, transform.rotation ) as UnitModel;
		if( model == null ) {
			throw new UnityException( "model is NULL, yo" );
		}
		model.transform.parent = transform;
		model.Init( this );
	}

	internal void SetMaterial( Material material ) {
		model.SetMaterial( material );
	}

}