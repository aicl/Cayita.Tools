
using System.Collections.Generic;

namespace Cayita.Tools.Auth
{
	public class AuthorizationResponse
	{
		public AuthorizationResponse ()
		{

			Permissions= new List<AuthPermission>();
			Roles = new List<AuthRole>();
		}
				
		public List<AuthPermission> Permissions {get; set;}
		public List<AuthRole> Roles {get; set;}
		
	}
}

