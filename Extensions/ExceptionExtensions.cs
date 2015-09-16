using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Seyyedi
{
	public static class ExceptionExtensions
	{
		static string CleanupStackTrace(string stackTrace)
		{
			if (string.IsNullOrEmpty(stackTrace))
			{
				return stackTrace;
			}

			var rAsync = new Regex(@"\<(.+)\>.+\.MoveNext");

			return stackTrace
				.Split('\n')
				.Where(l =>
					!l.StartsWith("   at System.", StringComparison.Ordinal)
					&& !l.StartsWith("---", StringComparison.Ordinal)
				)
				.Select(l => rAsync.Replace(l, "$1"))
				.Join("\n");
		}

		public static string PrettyFormat(this Exception exception)
		{
			var sb = new StringBuilder();
			var ex = exception;

			while (ex != null)
			{
				sb.AppendFormat("[{0}] {1}\n{2}", ex.GetType().FullName, ex.Message, CleanupStackTrace(ex.StackTrace));
				ex = ex.InnerException;

				if (ex != null)
				{
					sb.AppendLine();
				}
			}

			return sb.ToString();
		}

		public static void PrettyFormat(this Exception exception, TextWriter writer)
			=> writer.Write(
				exception.PrettyFormat()
			);
	}
}