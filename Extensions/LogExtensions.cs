using System;
using System.Diagnostics;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using log4net.Appender;
using log4net.Core;

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

		public static void BindConsole(PatternLayout layout = null, Level level = null)
		{
			layout = layout ?? new PatternLayout
			{
				ConversionPattern = "%date{ISO8601} %level %logger - %message%newline"
			};

			layout.ActivateOptions();

			var consoleAppender = new ConsoleAppender
			{
				Name = "console",
				Layout = layout
			};

			consoleAppender.ActivateOptions();

			var hierarchy = (Hierarchy)LogManager.GetRepository();

			if (hierarchy.Root.GetAppender("console") != null)
			{
				return;
			}

			hierarchy.Root.AddAppender(consoleAppender);
			hierarchy.Root.Level = level ?? Level.Info;

			hierarchy.Configured = true;
		}
	}
}