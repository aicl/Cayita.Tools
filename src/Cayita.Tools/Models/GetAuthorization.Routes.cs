using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{
	[Route("/Authorization/read","POST,GET")]
	public  class GetAuthorization:IReturn<AuthorizationResponse>
	{
		public GetAuthorization ()
		{
		}
		
		public int UserId{ get; set;}
	}
}

