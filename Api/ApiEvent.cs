namespace Seyyedi.Api
{
	public class ApiEvent : ApiEvent<object>
	{
		
	}

	public class ApiEvent<T>
	{
		public string Id { get; set; }
		public T Data { get; set; }
	}
}
