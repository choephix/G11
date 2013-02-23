using System.Collections.Generic;
using UnityEngine;

public class MissionBaseClass : BaseClass {

	protected static God god;
	public static GodOfTheStage stage;
	public static FreeCameraController freeCameraHolder;
	public static SmartCamera smartCamera;
	public static MissionGUI gui;

	public static GodOfProcesses processManager;

	public static List<Team> allTeams;
	public static List<Unit> allUnits;
	public static Grid grid;

	public static Unit selectedUnit;
	public static Unit targetedUnit;
	public static Action selectedAction;

	public static Transform markerTargetable;

	public static implicit operator Transform( MissionBaseClass instance ) {
		return instance.transform;
	}
	public static implicit operator GameObject( MissionBaseClass instance ) {
		return instance.gameObject;
	}

}