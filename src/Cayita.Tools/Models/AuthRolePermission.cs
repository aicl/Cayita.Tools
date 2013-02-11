using ServiceStack.DesignPatterns.Model;
using ServiceStack.DataAnnotations;

namespace Cayita.Tools.Auth
{
	public partial class AuthRolePermission:IHasIntId{
		
		public AuthRolePermission(){}
		
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set;} 
		
		public int AuthRoleId { get; set;} 
		
		public int AuthPermissionId { get; set;} 
		
	}
}

