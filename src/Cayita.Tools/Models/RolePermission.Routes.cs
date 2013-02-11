using System;
using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{

	[Route("/RolePermission/read","POST, GET")]
	public class GetRolePermission:IReturn<AuthResponse<RolePermission>>
	{
		public int AuthRoleId{get;set;}
	}

	[Route("/RolePermission/create","POST")]
	public class CreateRolePermission:IReturn<AuthResponse<RolePermission>>
	{
		public int AuthRoleId { get; set;} 
		public int AuthPermissionId { get; set;} 
	}


	[Route("/RolePermission/destroy","POST,DELETE")]
	public class DeleteRolePermission:IReturnVoid
	{
		
		public int Id { get; set;} 

	}

}

