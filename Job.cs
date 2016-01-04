using System;
using System.Threading.Tasks;

namespace Seyyedi
{
	public class IndexInvoicesJob : IndexJob<IndexInvoicesJob.Options>
	{
		public class Options : IndexOptions
		{
			public long? ClientId { get; set; }
			public long? InvoiceId { get; set; }
		}

		public override async Task Run(Options options)
		{
			if (options.All.HasValue)
			{

			}
			else if (options.ClientId.HasValue)
			{

			}

			await Task.Delay(1000);
		}
	}

	public class DeleteInvoiceIndexJob : Job
	{
		public override async Task Run(JobOptions options)
		{
			await Task.Delay(2000);
		}
	}

	public abstract class IndexJob<T> : Job<T>
		where T : IndexJob<T>.IndexOptions
	{
		public class IndexOptions : JobOptions
		{
			public bool? All { get; set; }
			public DateTime? Since { get; set; }
		}
	}

	public abstract class Job : Job<JobOptions>
	{

	}

	public abstract class Job<T>
		where T : JobOptions
	{
		public abstract Task Run(T options);
	}

	public class JobOptions
	{
		public JobRetryBehaviour RetryBehaviour { get; set; }
	}

	public enum JobRetryBehaviour
	{
		None,
		Once,
		Forever
	}
}
