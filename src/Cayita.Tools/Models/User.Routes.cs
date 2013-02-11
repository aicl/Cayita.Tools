using System;
using ServiceStack.ServiceHost;

namespace Cayita.Tools.Auth
{
	[Route("/User/read","POST,GET")]
	public class GetUser:IReturn<AuthResponse<User>>
	{
		public int? Id {get;set;}
		public string UserName {get;set;}
	}


	[Route("/User/create","POST")]
	public class CreateUser:IReturn<AuthResponse<User>>
	{

		public string UserName { get; set; } 
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
			
		public string Password {get ;set;}

		public string Info {get ;set;}
		public bool IsActive {get ;set;}
		public DateTime? ExpiresAt {get ;set;}

	}

	[Route("/User/update","POST,PUT")]
	public class UpdateUser:IReturn<AuthResponse<User>>
	{

		public int Id {get;set;}
		public string UserName { get; set; } 
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		
		public string Password {get ;set;}
		
		public string Info {get ;set;}
		public bool IsActive {get ;set;}
		public DateTime? ExpiresAt {get ;set;}
		
	}

	[Route("/User/destroy","POST,DELETE")]
	public class DeleteUser:IReturnVoid
	{
		public int Id {get;set;}
	}



}

