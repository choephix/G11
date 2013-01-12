using UnityEngine;
using System.Collections;

public class GodOfVisualEffects : MissionBaseClass {

	public GUITexture vignette;
	public Texture vignetteRegular;
	public Texture vignetteCinematic;

	public Transform markerSelectedUnit;
	public Transform markerTargetedUnit;

	void Start() {
		GameMode.eventChanged += OnGameModeChanged;
		SelectionManager.UnitSelectedEvent += PlaceMarkerSelected;
		SelectionManager.UnitTargetedEvent += PlaceMarkerTargeted;
		//SelectionManager.UnitUnselectedEvent += OnUnitUnselected;
		//SelectionManager.UnitUntargetedEvent += OnUnitUntargeted;
		markerSelectedUnit.gameObject.SetActive( false );
		markerTargetedUnit.gameObject.SetActive( false );
	}

	void Update() {
		vignette.texture = GameMode.cinematic ? vignetteCinematic : vignetteRegular;
		markerSelectedUnit.gameObject.SetActive( GameMode.selecting );
		markerTargetedUnit.gameObject.SetActive( GameMode.targeting );
		if( selectedUnit ) {
			PlaceMarkerSelected( selectedUnit );
		}
	}

	public void OnGameModeChanged( GameModes newMode, GameModes oldMode ) {
	}

	public void PlaceMarkerSelected( Unit unit ) {
		markerSelectedUnit.position = unit.transform.position;
		markerSelectedUnit.rotation = unit.transform.rotation;
	}

	public void PlaceMarkerTargeted( Unit unit ) {
		markerTargetedUnit.position = unit.transform.position;
		markerTargetedUnit.rotation = unit.transform.rotation;
	}

}
