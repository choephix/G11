using UnityEngine;
using System.Collections;

public class SmartCamera : MissionBaseClass {

	public static SmartCamera me;
	public static float defaultDamping = .5f;

	public Camera theCamera;
	public Light torch;
	public Transform lookTarget;
	public Transform defaultDestination;
	public Transform defaultLookTargetDestination;

	private Transform destination;
	private Transform lookTargetDestination;

	private float damping = defaultDamping;
	private Vector3 localDestination = Vector3.zero;
	
	void Start () {
		me = this;
		ResetTargetHolder();
		SelectionManager.UnitTargetedEvent += OnUnitTargeted;
	}
	
	void Update () {
		if( damping > 0 ) {
			transform.position = Vector3.Lerp( transform.position, destination.position, 1 - damping );
			lookTarget.position = Vector3.Lerp( lookTarget.position, lookTargetDestination.position, 1 - damping );
			theCamera.transform.localPosition = Vector3.Lerp( theCamera.transform.localPosition, localDestination, 1 - damping );
		} else {
			instantMoveToDestination();
			instantMoveTargetToDestination();
		}

		transform.LookAt(lookTarget, Vector3.up);
		//if( God.selectedUnit ) {
		//    torch.transform.LookAt( God.selectedUnit.spots.torso );
		//}
		torch.transform.LookAt( lookTarget );

	//	updateObjectVisibilities();

	}

	private static Vector3 tempV;
	internal void traverse( float x, float y, float zoom = 0.0f ) {
		if( x != 0.0f || y != 0.0f ) {
			tempV = new Vector3( x, 0.0f, y );
			smartCamera.defaultDestination.position += tempV;
			smartCamera.defaultLookTargetDestination.position += tempV;
		}
		if( zoom != 0.0f ) {
			tempV = new Vector3( 0.0f, zoom, 0.0f );
			if( zoom < 0.0f && smartCamera.defaultDestination.position.y < -zoom ) {
				return;
			}
			if( zoom > 0.0f && smartCamera.defaultDestination.position.y > 10.0f ) {
				return;
			}
			smartCamera.defaultDestination.position += tempV;
		}
	}

	private void instantMoveToDestination() {
		transform.position = destination.position;
		theCamera.transform.localPosition = localDestination;
	}

	private void instantMoveTargetToDestination() {
		lookTarget.position = lookTargetDestination.position;
	}

	internal void moveTo( Transform target ) {
		smartCamera.traverse(
			target.position.x - smartCamera.defaultLookTargetDestination.transform.position.x,
			target.position.z - smartCamera.defaultLookTargetDestination.transform.position.z
			);
	}

	internal void updateObjectVisibilities() {
		foreach( WorldObject o in GodOfTheStage.objects ) {
			if( o != null ) {
				if( Vector3.Distance( theCamera.transform.position, o.transform.position ) < .59 ) {
					o.model.enabled = false;
				} else {
					o.model.enabled = true;
				}
			} else {
				print( o );
			}
		}
	}

	public void setDestinations( Transform destination, Transform lookTargetDestination, float destinationZDistance = 0, float damping = -1 ) {
		if( damping < 0 ) {
			damping = defaultDamping;
		}
		this.damping = damping;
		this.destination = destination;
		this.lookTargetDestination = lookTargetDestination;
		this.localDestination = new Vector3( theCamera.transform.localPosition.x, theCamera.transform.localPosition.y, -destinationZDistance );
	}

	public void ResetTargetHolder( bool instant = false ) {
		damping = defaultDamping;
		setDestinations( defaultDestination, defaultLookTargetDestination, 0, instant ? 0 : .5f );
	}

	internal void OnModeChange( byte newMode, byte oldMode ) {

		switch( newMode ) {
			case CameraMode.TARGET:
				break;
			case CameraMode.RUN:
				setDestinations( God.selectedUnit.currentTile.cameraSpot, God.selectedUnit.spots.torso, 3, .9f );
				instantMoveToDestination();
				//if( Chance( 100 ) ) {
				//    if( Chance( 50 ) ) {
				//        setDestinations( God.selectedUnit.spots.back, God.selectedUnit.spots.forth, 1.5f, 0 );
				//    } else {
				//        setDestinations( God.selectedUnit.currentTile.cameraSpot, God.selectedUnit.spots.wayBack, 3, 0 );
				//    }
				//}
				GameMode.cinematic = true;
				break;
			case CameraMode.FREE:
				God.smartCamera.ResetTargetHolder();
				GameMode.cinematic = false;
				break;
			default:
				break;
		}

	}

	internal void OverTheShoulderAndLookTarget( bool rightSide ) {
		if( rightSide ) {
			setDestinations( God.selectedUnit.spots.right, God.targetedUnit.model.torso, 1f, .8f );
		} else {
			setDestinations( God.selectedUnit.spots.left, God.targetedUnit.model.torso, 1f, .8f );
		}
		//	setDestinations( God.selectedUnit.spots.overHead, God.targetedUnit.model.torso, 1f, .5f );
	}

	// EVENT HANDLERS

	internal void OnUnitTargeted( Unit unit ) {

		OverTheShoulderAndLookTarget( selectedUnit.actionSide );

	}

}

public static class CameraMode : object {

	internal const byte FREE		= 0;
	internal const byte TARGET		= 1;
	internal const byte CINEMATIC	= 2;
	internal const byte RUN			= 3; //TODO NewMove the run cinematic to GodOfCinematics
	internal const byte DEFAULT		= 127;

	internal static bool cinematic = false;

	private static byte @default = FREE;
	private static byte current = @default;

	internal static bool Is( byte value ) {
		return current == value;
	}

	internal static byte Get() {
		return current;
	}

	internal static bool Set( byte value ) {

		if( value == DEFAULT ) {
			value = @default;
		}

		if( value == current ) {
			return false;
		} else {
			SmartCamera.me.OnModeChange( value, current );
			current = value;
			return true;
		}

	}

	internal static bool Toggle( byte value ) {
		if( value == current ) {
			Set( DEFAULT );
			return false;
		} else {
			Set( value );
			return true;
		}
	}

	internal static void SetDefault( byte value ) {
		@default = value;
	}

	internal static void Reset() {
		Set( @default );
	}

}