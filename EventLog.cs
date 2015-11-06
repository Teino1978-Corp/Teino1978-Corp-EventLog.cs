class EventLog {
	class Event {
		DateTime when;
		// can track when event started as well, lets you loop back through and find failed tasks
		Key key;
	}
	
	Event[] events;
	int nextPush = 0;
	int nextPop = 0;
	private Object _popLock = new Object();
	private Object _punhLock = new Object();
	
	EventLog(int totalEvents) {
		// easy to statically size this ahead of time
		// if static size, know there's no contention for size
		// otherwise, you can do circular buffer of count(distinct(key))
		events = new Event[totalEvents];
	}
	
	public Event pop() {
		Event e;
		while(!tryPop(ref e)) {
			Thread.Sleep(100); // can do sempahore or something better than spinwait
		}
		
		if (e.when > now)
			sleepUntil(e.when);
		return e;
	}
	
	private bool tryPop(ref Event e) {
		lock (_popLock) {
			if(nextPop >= nextPush)
				return false;
			
			e = this.events[nextPop++];
			return true;
		}
	}
	
	public void push(Key key) {
		Event e = new Event { now() + delay, key };
		lock (_pushLock) {
			this.events[nextPush++] = e;
		}
	}
}