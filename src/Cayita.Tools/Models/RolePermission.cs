using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace Cayita.Tools.Auth
{
	[JoinTo(typeof(AuthPermission),"AuthPermissionId","Id")]
	[Alias("AuthRolePermission")]
	public class RolePermission:IHasIntId
	{
		public RolePermission ()
		{
		}
		
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set;} 
		
		public int AuthRoleId { get; set;} 
		
		public int AuthPermissionId { get; set;} 
		
		[BelongsTo(typeof(AuthPermission))]
		public string Name {get;set;}
		
	}
}

