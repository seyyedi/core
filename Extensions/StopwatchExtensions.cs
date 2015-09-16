using System;
using System.Diagnostics;

namespace Seyyedi
{
	public static class StopwatchExtensions
	{
		public static void Restart(this Stopwatch stopwatch)
		{
			stopwatch.Reset();
			stopwatch.Start();
		}

		public static void Measure(this Stopwatch stopwatch, Action method)
		{
			stopwatch.Restart();
			method();
			stopwatch.Stop();
		}
	}
}
