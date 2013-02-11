using System;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Cayita.Tools.Auth
{
	public partial class AuthRole:IHasIntId
	{
		
		public AuthRole(){}
		
		[PrimaryKey]
		[AutoIncrement]
		public Int32 Id { get; set;} 
		
		[Required]
		[StringLength(30)]
		public String Name { get; set;} 
		
		[StringLength(15)]
		public String Directory { get; set;} 
		
		[StringLength(2)]
		public String ShowOrder { get; set;} 
		
		[Required]
		[StringLength(30)]
		public String Title { get; set;} 
		
	}
}

