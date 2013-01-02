using UnityEngine;
using System.Collections;

public class GodOfVisualEffects : MissionBaseClass {

	public GUITexture vignette;
	public Texture vignetteRegular;
	public Texture vignetteCinematic;

	public Transform markerSelectedUnit;
	public Transform markerTargetedUnit;

	public Transform rangePlane;

	void Start() {
		GameMode.eventChanged += OnGameModeChanged;
		SelectionManager.UnitSelectedEvent += PlaceMarkerSelected;
		SelectionManager.UnitTargetedEvent += PlaceMarkerTargeted;
		//SelectionManager.UnitUnselectedEvent += OnUnitUnselected;
		//SelectionManager.UnitUntargetedEvent += OnUnitUntargeted;
		markerSelectedUnit.gameObject.SetActiveRecursively( false );
		markerTargetedUnit.gameObject.SetActiveRecursively( false );
	}

	void Update() {
		vignette.texture = GameMode.cinematic ? vignetteCinematic : vignetteRegular;
		markerSelectedUnit.gameObject.SetActiveRecursively( GameMode.selecting );
		markerTargetedUnit.gameObject.SetActiveRecursively( GameMode.targeting );
		if( selectedUnit ) {
			PlaceMarkerSelected( selectedUnit );
		}
	}

	public void OnGameModeChanged( GameModes newMode, GameModes oldMode ) {
	}

	public void PlaceMarkerSelected( Unit unit ) {
		markerSelectedUnit.position = unit.transform.position;
		markerSelectedUnit.rotation = unit.transform.rotation;
		//markerSelectedUnit.localScale = unit.transform.localScale;
		//markerSelectedUnit.parent = unit.transform;

		//rangePlane.renderer.enabled = GameMode.Is( GameModes.SELECT ) && selectedUnit;
		//rangePlane.localScale = Vector3.one * selectedUnit.propAttackRange;
	}

	public void PlaceMarkerTargeted( Unit unit ) {
		markerTargetedUnit.position = unit.transform.position;
		markerTargetedUnit.rotation = unit.transform.rotation;
		//markerTargetedUnit.localScale = unit.transform.localScale;
		//markerTargetedUnit.parent = unit.transform;
		//omni3.range = unit.transform.localScale.z * 1.5f;
	}

}
