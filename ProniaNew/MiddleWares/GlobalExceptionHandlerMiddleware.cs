namespace ProniaNew.MiddleWares
{
	public class GlobalExceptionHandlerMiddleware
	{
		private readonly RequestDelegate _next;

		public GlobalExceptionHandlerMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task InvokeAsync(HttpContext context)
		{

			try
			{
				await _next.Invoke(context);
			}
			catch (Exception except)
			{
				context.Response.Redirect($"/Home/ErrorPage?error={except.Message}");
			}
		}
	}
}
	}
}
