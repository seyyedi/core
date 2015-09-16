using System;
using System.Diagnostics;
using log4net;

namespace Seyyedi
{
	public static class LogExtensions
	{
		public static void Exception(this ILog log, Exception ex)
			=> log.Error(
				ex.PrettyFormat()
			);
	}

	public static class Logs
	{
		public static ILog Get()
			=> Get(
				new StackTrace().GetFrame(1).GetMethod().ReflectedType
			);

		public static ILog Get(string name)
			=> LogManager.GetLogger(name);

		public static ILog Get(Type type)
			=> LogManager.GetLogger(type);
	}
}