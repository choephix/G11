using UnityEngine;
using System.Collections;

public class GodOfData : MonoBehaviour {


	public static GodOfData me;

	public SaveData data;

	public SaveData data2;

	public static SaveData staticData;

	public GodOfData() {
		if( me == null ) {
			me = this;
		} else {
			Debug.LogWarning( "GodOfData singleton was already initialized but tried again.." );
		}
	}


}

public static class ConvertSaveData {

	public static WeaponData From( Weapon weapon ) {
		return new WeaponData();
	}

	public static UnitData From( UnitInitProps initProps ) {
		return new UnitData();
	}

	//public static UnitData From( UnitInitProps teamProps ) {
	//    return new UnitData();
	//}

	public static UnitInitProps From( UnitData unitData ) {
		return new UnitInitProps();
	}

}



[System.Serializable]
public class SaveData {

	public WeaponData[] myArmory;

	public UnitData[] myUnits;

}


[System.Serializable]
public class WeaponData {

	public int uid = 0;

	public int weaponN = 0;

	public int ammoN = 1;

	public int[] addons = { 2, 3 };

}

[System.Serializable]
public class UnitData {

	public string name = "Gago";

	public int accuracy = 99;

	public int modelN = 0;

	public int[] equipment = { 0, 1, 4 };

	public WeaponData primaryWeapon;

}