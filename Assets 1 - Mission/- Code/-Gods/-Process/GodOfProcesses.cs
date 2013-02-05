using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class GodOfProcesses : MissionBaseClass {

	protected readonly List<Watcher> watchers = new List<Watcher>();

	protected readonly List<Process> waitingList = new List<Process>();
	protected readonly List<Process> activePool = new List<Process>();
	
	public bool empty { get { return activePool.Count == 0; } }
	public bool interactive { get; private set; }


	void Awake() {

		processManager = this;

	}

	void Update() {

		activePool.RemoveAll( p => p.ended );

		if( empty ) {
			
			if( waitingList.Count > 0 ) {

				AddImmediately( waitingList[0] );
				waitingList.RemoveAt( 0 );

			}

		}
			
		while( activePool.Exists( p => !p.@checked ) ) {
			Process process = activePool.Find( p => !p.@checked );
			process.@checked = true;
			OnProcessWillStart( process );
		}

		activePool.ForEach( p => { if( p.@checked ) UpdateProcess(p); } );

	}

	static void UpdateProcess( Process process ) {

		process.Update();

	}

	private void OnChanged() {

		interactive = activePool.FindAll( p => !p.background ).Count == 0;
		interactive = true;

	}

	//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//

	public void Add( Process process ) {
		waitingList.Add( process );
	}

	public void AddImmediately( Process process ) {
		activePool.Add( process );
		OnChanged();
	}

	public void AddImmediately( IEnumerable<Process> processes ) {
		activePool.AddRange( processes );
		OnChanged();
	}

	public void OvertakeAdd( Process overtaker, Process overtakee ) {
		Postpone( overtakee );
		JumpAdd( overtaker );
	}

	public void JumpAdd( Process process ) {
		waitingList.Insert( 0, process );
	}

	public void Postpone( Process process ) {

		if( !activePool.Contains( process ) ) return;

		process.@checked = false;
		activePool.Remove( process );
		JumpAdd( process );
		OnChanged();

	}

	//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//

	private void OnProcessWillStart( Process process ) {

		Debug.Log( "PROCESS " + process + " is about to start, checking for counters.." );

		foreach( Watcher watcher in watchers ) {

			watcher.OnProcessWillStart( process );

		}

		CleanUpWatchers();

	}

	public void Add( Watcher watcher ) {

		Debug.Log( "Adding new watcher " + watcher + " to the watchers pool" );

		watchers.Add( watcher );

	}

	private void CleanUpWatchers() {

		watchers.ForEach( w => { if( w.expired ) Debug.Log( "Watcher " + w + " expired and will be removed" ); } );

		watchers.RemoveAll( w => w.expired );

	}

	//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//**\\**//

	//public static GodOfProcesses operator +( GodOfProcesses pq, Process p ) { pq.Add( p ); return pq; }

	public static GodOfProcesses operator +( GodOfProcesses pq, Watcher w ) { pq.Add( w ); return pq; }

}
