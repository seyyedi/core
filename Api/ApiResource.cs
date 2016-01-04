namespace Seyyedi.Api
{
	public class ApiResource<T>
		where T : IResourceEntity
	{
		public string Id { get; set; }
		public T Data { get; set; }
	}

	public interface IResourceEntity
	{
		long Id { get; }
	}
}
