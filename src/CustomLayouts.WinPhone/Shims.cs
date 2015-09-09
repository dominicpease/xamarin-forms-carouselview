using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace System.Timers
{
	public delegate void ElapsedEventHandler(object sender, ElapsedEventArgs e);

	public class ElapsedEventArgs : EventArgs
	{
		public ElapsedEventArgs(DateTime signal) { SignalTime = signal; }
		public DateTime SignalTime { get; private set; }
	}

	internal class Timer
	{
		DispatcherTimer _timer;

		public Timer()
		{
			_timer = new DispatcherTimer();
			_timer.Tick += _timer_Tick;
		}

		public Timer(int ms)
			: this()
		{
			Interval = ms;
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			Stop();

			if (Elapsed != null)
				Elapsed(this, new ElapsedEventArgs(DateTime.Now));

			if (AutoReset)
				Start();
		}

		public bool AutoReset { get; set; }
		public double Interval { get { return _timer.Interval.TotalMilliseconds; } set { _timer.Interval = TimeSpan.FromMilliseconds(value); } }

		public void Start() { _timer.Start(); }
		public void Stop() { _timer.Stop(); }

		public event ElapsedEventHandler Elapsed;
	}
}
