namespace RSA_UI.Models.Response
{
	public class Response<T>
	{
		public T? Data { get; set; } = default(T);
		public int StatusCode { get; set; } = 200;
		public string Message { get; set; } = string.Empty;
		public Response() { }
        public Response(T? data, int statusCode, string message)
		{
			Data = data;
			StatusCode = statusCode;
			Message = message;
		}
		public string GetAlertLevel()
		{
			return StatusCode switch
			{
				200 => "success",
				_ => "danger"
			};
		}
	}

	public class ResponseList<T> : Response<T>
	{
		public List<T> List { get; set; } 

		public ResponseList(T? data, int statusCode, string message, List<T> listValues) : base(data, statusCode, message)
		{
			List = listValues;
		}
	}
}
