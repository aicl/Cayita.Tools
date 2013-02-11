using System;
using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{
	[Route("/AuthPermission/read","POST,GET")]
	public class GetAuthPermission:IReturn<AuthResponse<AuthPermission>>
	{
		public GetAuthPermission ()
		{
		}

		public String Name { get; set;} 
	}

	[Route("/AuthPermission/create","POST")]
	public class CreateAuthPermission:IReturn<AuthResponse<AuthPermission>>
	{
		public CreateAuthPermission ()
		{
		}
		
		public String Name { get; set;} 
	}

	[Route("/AuthPermission/update","POST,PUT")]
	public class UpdateAuthPermission:IReturn<AuthResponse<AuthPermission>>
	{
		public UpdateAuthPermission ()
		{
		}
		
		public String Name { get; set;} 
	}

	[Route("/AuthPermission/destroy","POST,DELETE")]
	public class DeleteAuthPermission:IReturnVoid
	{
		public DeleteAuthPermission ()
		{
		}
		
		public int Id { get; set;} 
	}

}

