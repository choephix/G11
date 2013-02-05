using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Process {

	public readonly string name;
	public readonly bool stackable;

	public bool started;
	public bool ended;
	public event EventHandler eventStarted = delegate { };
	public event EventHandler eventEnded = delegate { };

	public bool hasAttached { get { return attached != null && attached.Count > 0; } }
	public bool hasEnqueued { get { return enqueued != null && enqueued.Count > 0; } }

	protected List<Process> attached;
	protected List<Process> enqueued;

	public bool @checked;
	public bool background;

	protected Process( string name, bool stackable = true ) {
		this.name = name;
		this.stackable = stackable;
	}

	public virtual void Update() {

		if( !started ) {
			Start();
		}

		if( !ended ) {
			_Update();
		}

	}

	protected virtual void _Update() {}

	protected void Start() {

		started = true;
		eventStarted.Invoke();
		Events.processStarted.Invoke( this );

		Debug.Log( "process " + this + " just started" );

		if( hasAttached ) {
			God.processManager.AddImmediately( attached );
			attached = null;
		}

		_Start();

	}

	protected virtual void _Start() { }

	protected void End() {

		_End();

		Debug.Log( "process " + this + " just ended" );

		if( hasEnqueued ) {
			God.processManager.AddImmediately( enqueued );
			enqueued = null;
		}

		ended = true;
		eventEnded.Invoke();
		Events.processFinished.Invoke( this );

	}

	protected virtual void _End() { }

	public void Terminate() {
		End();
	}

	public Process Attach( Process process ) {

		if( attached == null ) {
			attached = new List<Process>();
		}

		attached.Add( process );

		return process;

	}

	public Process Enqueue( Process process ) {

		if( enqueued == null ) {
			enqueued = new List<Process>();
		}

		enqueued.Add( process );

		return process;

	}

	public string ToString( bool wholeTree ) {

		if( !wholeTree || ( !hasEnqueued && !hasAttached ) ) return ToString();

		string result = ToString();

		if( hasAttached ) {
			result += "\n:ATTACHED:";
			result = attached.Aggregate( result , ( current , process ) => current + ( "[" + process.ToString( true ) + "]" ) );
		}

		if( hasEnqueued ) {
			result += "\n:ENQUEUED:";
			result = enqueued.Aggregate( result , ( current , process ) => current + ( "[" + process.ToString( true ) + "]" ) );
		}

		return result;

	}

	public override string ToString() {

		return name + (started?" >>":"");

	}

	//-- -- -- -- -- -- -- -- -- -- -- TREE

}