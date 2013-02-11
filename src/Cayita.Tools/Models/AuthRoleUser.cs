using System;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.DataAnnotations;

namespace Cayita.Tools.Auth
{
	public partial class AuthRoleUser:IHasIntId, IHasIntUserId{
		
		public AuthRoleUser(){}
		
		[PrimaryKey]
		[AutoIncrement]
		public Int32 Id { get; set;} 
		
		public Int32 AuthRoleId { get; set;} 
		
		public Int32 UserId { get; set;} 
		
	}

}

