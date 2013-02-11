using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace Cayita.Tools.Auth
{
	[JoinTo(typeof(AuthRole),"AuthRoleId", "Id")]
	[Alias("AuthRoleUser")]
	
	public class UserRole:IHasIntId,IHasIntUserId
	{
		public UserRole ()
		{
		}
		
		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set;} 
		
		public int AuthRoleId { get; set;} 
		
		public int UserId { get; set;} 
		
		[BelongsTo(typeof(AuthRole))]
		public string Name{get; set;}
		
	}
}

