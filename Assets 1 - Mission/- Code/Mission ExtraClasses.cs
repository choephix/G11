using System.Collections;

public class GameMode {

	internal static EventHandler<GameModes, GameModes> eventChanged;

	internal static bool cinematic;
	internal static bool selecting { get { return !targeting; } }
	internal static bool targeting { get { return Is( GameModes.PickUnit ); } }
	internal static bool interactive {
		get {
			return
				( !Is( GameModes.Disabled ) && !Is( GameModes.GameOver ) && !cinematic && God.processManager.interactive );
		}
	}

	private static GameModes @default = GameModes.Disabled;
	private static GameModes current = @default;
	private static GameModes last =  GameModes.Normal;

	internal static bool Is( GameModes value ) {
		return current == value;
	}

	internal static GameModes Get() {
		return current;
	}

	internal static bool Set( GameModes value ) {

		if( !Is( GameModes.GameOver ) ) {

			if( value == GameModes.Default ) {
				value = @default;
			}

			if( value == GameModes.Disabled && current != GameModes.Disabled ) {
				last = current;
			}

			if( value == current ) {
				return false;
			}

			eventChanged( value, current );
			current = value;
			return true;

		}

		return false;

	}

	internal static bool Toggle( GameModes value ) {

		if( value == current ) {
			Set( GameModes.Default );
			return false;
		}

		Set( value );
		return true;

	}

	internal static void Disable() {
		Set( GameModes.Disabled );
	}

	internal static void Reenable() {
		Set( last );
	}

	internal static void SetDefault( GameModes value ) {
		@default = value;
	}

	internal static void Reset() {
		Set( @default );
		if( God.selectedUnit ) {
			God.selectedUnit.OnUntargetingUnit(); //REFACTOR THIS CRAP
		}
		GodOfTime.speed = 1f;
	}

}