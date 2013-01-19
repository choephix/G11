public abstract class Watcher {

	public event EventHandler<Process> eventWillStart;

	public bool eternal = false;
	public int instances = 1;

	public bool expired;

	public abstract void OnProcessWillStart( Process process );

	protected void Fire( Process process ) {

		eventWillStart.Invoke( process );

	}

	protected void Terminate() {

		expired = true;

	}

}

public class Watcher<T> : Watcher {

	public override void OnProcessWillStart( Process process ) {

		if( !( process is T ) ) return;

		Fire( process );

		instances--;

		if( instances <= 0 ) {

			Terminate();

		}

	}

}

public class WatchingUnit<T> : Watcher<T> {

	private readonly Unit watchingUnit;

	public WatchingUnit( Unit watchingUnit, bool terminateNextTurn = true, bool terminateOnAction = false ) {

		this.watchingUnit = watchingUnit;

		Events.unitDied += CheckTerminationConditions;

		if( terminateNextTurn ) {

			Events.unitTurnStarted += CheckTerminationConditions;

		}

		if( terminateOnAction ) {

			Events.unitActionStarted += CheckTerminationConditions;

		}

	}

	private void CheckTerminationConditions( Unit unit ) {
		CheckTerminationConditions( unit , null ); 
	}

	private void CheckTerminationConditions( Unit unit, object beatsmewhat ) {

		if( unit != watchingUnit ) return;

		CleanupAndTerminate();

	}

	private void CleanupAndTerminate() {

		Events.unitTurnStarted -= CheckTerminationConditions;
		Events.unitActionStarted -= CheckTerminationConditions;
		Events.unitDied -= CheckTerminationConditions;
		Terminate();

	}

}