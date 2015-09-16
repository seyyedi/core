using System;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Seyyedi
{
	public class ConsoleProgram<T>
		where T : Application
	{
		static readonly ILog Log = Logs.Get();

		public static void Run(string[] args)
		{
			RunAsync(args).Wait();
		}

		static async Task RunAsync(string[] args)
		{
			try
			{
				var program = new ConsoleProgram<T>();
				program.Configure();

				var application = program.LoadApplication(args);

				if (application == null)
				{
					Log.Error("No application loaded");
					return;
				}

				await Run(application);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				if (Environment.UserInteractive)
				{
					Console.ReadLine();
				}
			}
		}

		static async Task Run(T application)
		{
			await application.Start();

			try
			{
				await application.Run();
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			if (Environment.UserInteractive)
			{
				Console.ReadLine();
			}

			await application.Stop();
		}

		protected virtual void Configure()
		{
			ConfigureLogging();
		}

		protected virtual void ConfigureLogging()
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();

			var layout = new PatternLayout
			{
				ConversionPattern = "%date{ISO8601} %level %logger - %message%newline"
			};

			layout.ActivateOptions();

			var consoleAppender = new ConsoleAppender
			{
				Layout = layout
			};

			consoleAppender.ActivateOptions();

			hierarchy.Root.AddAppender(consoleAppender);

			hierarchy.Root.Level = Level.Info;
			hierarchy.Configured = true;
		}

		protected virtual T LoadApplication(string[] args)
		{
			var application = Activator.CreateInstance<T>();
			application.Arguments = args;

			return application;
		}
	}
}
