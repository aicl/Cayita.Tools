using System;
using System.Collections.Generic;

namespace Cayita.Tools.Auth
{
	public class LoginResponse
	{
		public LoginResponse ()
		{
			Permissions= new List<AuthPermission>();
			Roles = new List<AuthRole>();	
			Info = new Dictionary<string, string>();
		}
		
				
		public List<AuthPermission> Permissions {get; set;}
		public List<AuthRole> Roles {get; set;}
		public string DisplayName { get; set;}
		public Dictionary<string,string> Info{get;set;}
		

	}
}

