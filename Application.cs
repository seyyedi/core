using System.Threading.Tasks;

namespace Seyyedi
{
	public class Application
	{
		public string[] Arguments { get; internal set; }

		public virtual Task Start()
		{
			return Task.FromResult(true);
		}

		public virtual Task Run()
		{
			return Task.FromResult(true);
		}

		public virtual Task Stop()
		{
			return Task.FromResult(true);
		}
	}
}
