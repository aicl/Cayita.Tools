using System;
using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{
	[Route("/AuthRole/read","POST,GET")]
	public class GetAuthRole:IReturn<AuthResponse<AuthRole>>
	{
		public GetAuthRole ()
		{
		}
		
		public String Name { get; set;} 
	}
	
	[Route("/AuthRole/create","POST")]
	public class CreateAuthRole:IReturn<AuthResponse<AuthRole>>
	{
		public CreateAuthRole ()
		{
		}
		
		public String Name { get; set;} 
		public String Directory { get; set;} 
		public String ShowOrder { get; set;} 
		public String Title { get; set;} 
	}
	
	[Route("/AuthRole/update","POST,PUT")]
	public class UpdateAuthRole:IReturn<AuthResponse<AuthRole>>
	{
		public UpdateAuthRole ()
		{
		}
		
		public String Name { get; set;}
		public String Directory { get; set;} 
		public String ShowOrder { get; set;} 
		public String Title { get; set;} 
	}
	
	[Route("/AuthRole/destroy","POST,DELETE")]
	public class DeleteAuthRole:IReturnVoid
	{
		public DeleteAuthRole ()
		{
		}
		
		public int Id { get; set;} 
	}
	
}

