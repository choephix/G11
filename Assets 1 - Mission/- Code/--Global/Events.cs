




// ReSharper disable FieldCanBeMadeReadOnly.Global

public static class Events {

	public static EventHandler turnStarted = delegate { };
	public static EventHandler turnEnded = delegate { };

	public static EventHandler<Process> processWillStart = delegate { };
	public static EventHandler<Process> processStarted = delegate { };
	public static EventHandler<Process> processFinished = delegate { };

	public static EventHandler calculateSelectablesStart = delegate { };
	public static EventHandler calculateSelectablesFinished = delegate { };

	public static EventHandler<Unit> unitTurnStarted = delegate { };
	public static EventHandler<Unit> unitTurnEnded = delegate { };
	public static EventHandler<Unit> unitSelected = delegate { };
	public static EventHandler<Unit> unitDeselected = delegate { };
	public static EventHandler<Unit> unitTargeted = delegate { };
	public static EventHandler<Unit> unitUntargeted = delegate { };
	public static EventHandler<Unit, Weapon> unitWeaponEquipped = delegate { };
	public static EventHandler<Unit, Weapon> unitWeaponUnequipped = delegate { };
	public static EventHandler<Unit, Action> unitActionStarted = delegate { };
	public static EventHandler<Unit, Action> unitActionFinished = delegate { };
	public static EventHandler<Unit, GridTile> unitTileReached = delegate { };
	public static EventHandler<Unit, GridTile> unitTileCurrentReached = delegate { };
	public static EventHandler<Unit, Unit> unitAttacked = delegate { }; //2nd param is attacker
	public static EventHandler<Unit, Unit> unitDamaged = delegate { }; //2nd param is damager
	public static EventHandler<Unit, Unit> unitDied = delegate { }; //2nd param is killer (if any)


}

// ReSharper restore FieldCanBeMadeReadOnly.Global