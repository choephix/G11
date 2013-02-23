using UnityEngine;
using System.Collections;

public static class Config {


	public static bool USE_FOG = false;

	public static bool GORE = true;

	public static int DEV_UNIT_SIGHT_RANGE = 24;
	public static int DEV_UNIT_MOVE_RANGE = 8;

	public static float DEV_HIT_CHANCE_MULTIPLIER = 1.25f;

	/// Number of actions a unit starts each turn with, before any effects and abilities are applied
	public static byte BASE_UNIT_ACTIONS = 22; //2

	/// Every unit in play has 100 accuracy
	public static bool OVERRIDE_HIT_CHANCE_ACCURACY = false;
	/// Distance penalty hit-chance does not apply
	public static bool OVERRIDE_HIT_CHANCE_DISTANCE = false;
	/// Covers do not apply to hit chance
	public static bool OVERRIDE_HIT_CHANCE_COVER = false;
	/// Covers do not apply to hit chance
	public static bool OVERRIDE_HIT_CHANCE_UNIT_SIZE = false;
	/// Covers do not apply to hit chance
	public static bool OVERRIDE_HIT_CHANCE_UNIT_EVASION = false;


	public static bool SHOW_PATH_ARROWS = false;



}

public static class Z {


}