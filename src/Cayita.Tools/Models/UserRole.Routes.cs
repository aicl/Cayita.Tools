using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{

	[Route("/UserRole/read","POST,GET")]
	public class GetUserRole:IReturn<AuthResponse<UserRole>>
	{
		public int? UserId { get; set;} 
	}

	[Route("/UserRole/create","POST")]
	public class CreateUserRole:IReturn<AuthResponse<UserRole>>
	{
		public int UserId { get; set;} 
		public int AuthRoleId { get; set;} 
	}

	[Route("/UserRole/destroy","POST,DELETE")]
	public class DeleteUserRole:IReturnVoid
	{
		public int Id { get; set;} 
	}

}

