using UnityEngine;
using System.Collections;

public class MissionInitProps : BaseClass {

	public Unit unitSample;

	public UnitModel defaultUnitModel;
	public Material defaultUnitMaterial;
	public UnitEquipment defaultEquipment;

	public LevelInitProps level;

	public Weapon[] weapons;

	public TeamInitProps[] teams;

	public Weapon randomWeapon { get { return weapons.GetRandom(); } }

}

[System.Serializable]
public class LevelInitProps {

	public Int2D dimensions;

}

[System.Serializable]
public class TeamInitProps {

	public string name;
	public Color color = Color.gray;
	public bool isUserControlled = false;

	public UnitInitProps[] units;
	public Int2D spawnTileCoordinates;

}

[System.Serializable]
public class UnitInitProps {
	
	public string name;
	public float size = 1;
	public int accuracy = 100;
	public int health = 8;
	public int armor = 1;

	public UnitEquipment equipment;

	public UnitModel model;
	public Material skin;

	public Teams team;
	
}
