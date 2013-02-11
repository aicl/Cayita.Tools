using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{
	[Route("/login", "POST,GET")]
	public class Login:IReturn<LoginResponse>
	{
		public Login (){}

		public string UserName{get ;set;}
		public string Password{get ;set;}
	}

	[Route("/logout", "POST,GET,DELETE")]
	public class Logout:IReturnVoid
	{
		public Logout(){}
	}
}

