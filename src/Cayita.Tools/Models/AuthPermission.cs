using System;
using System.ComponentModel.DataAnnotations;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace Cayita.Tools.Auth
{
	
	public partial class AuthPermission:IHasIntId
	{
		
		public AuthPermission(){}
		
		[PrimaryKey]
		[AutoIncrement]
		public Int32 Id { get; set;} 
		
		[Required]
		[StringLength(30)]
		public String Name { get; set;} 
		
	}
}