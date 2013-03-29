using System;
using ServiceStack.ServiceHost;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.DesignPatterns.Model;
using System.Reflection;
using ServiceStack.Common.Utils;
using Cayita.Tools;
using Cayita.Tools.Auth;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;

namespace System.Collections.Generic
{
	public static partial class Extensions
	{
		public static TList<T> ConvertToTList<T>(this List<T> source)
		{
			TList<T> t = new TList<T> ();
			t.Result = source;
			return t;
		}
	}

}

namespace ServiceStack.ServiceHost
{
	public static partial class Extensions
	{
		public static Pager BuildPager(this IHttpRequest httpRequest){

			var pager = new Pager();
			int page;
						
			if (int.TryParse( httpRequest.QueryString["page"], out page))
				pager.PageNumber=page-1;
			
			int limit;
			
			if (int.TryParse( httpRequest.QueryString["limit"], out limit))
				pager.PageSize=limit;
			
			return pager;
		}

		public static bool IsCayita(this IHttpRequest httpRequest)
		{
			bool cayita=false;
			bool.TryParse(httpRequest.QueryString["cayita"], out cayita);
			return cayita;
		}
	}
}

namespace ServiceStack.OrmLite{

	public static partial class Extensions
	{
				
		//public static List<T> GetListFromCache<T>(this IDbConnection dbConn) where T: new()
		//{
		//	return dbConn.Select<T>();
		//}
		
		public static void InsertAndAssertId<T>(this IDbConnection dbConn,  T request, SqlExpressionVisitor<T> visitor=null ) 
			where T: IHasIntId, new()
		{
			if (visitor == null)
				dbConn.Insert<T> (request);
			else
				dbConn.InsertOnly<T> (request, visitor);
			
			if( request.Id==default(int))
			{
				Type type = typeof(T);
				PropertyInfo pi= type.GetPropertyInfo( OrmLiteConfig.IdField );
				var li = dbConn.GetLastInsertId();
				ReflectionUtils.SetProperty(request, pi, Convert.ToInt32(li));  
			}
			
		}
		
/*
		public static long Count<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate )
			where T: IHasIntId, new()
		{
			return  dbCmd.GetScalar<T,long>(
				e=>  Sql.Count(e.Id),
				predicate );
		}
*/
					
	}

}

namespace ServiceStack.ServiceInterface{

	public static partial class Extensions
	{
		public static string GetUserUrn(this IServiceBase authService )
		{
			return string.Format("urn:{0}:UserData:{1}:{2}", authService.GetSessionId() , typeof(User).Name,"_user_");
		}

		public static string GetUserDataUrn<T>(this IServiceBase authService,  string key){
			return string.Format("urn:{0}:UserData:{1}:{2}", authService.GetSessionId(), typeof(T).Name, key);
		}

	}

}


namespace ServiceStack.Redis{
	public static partial class Extensions
	{
		public static User GetUser(this IRedisClient redisClient, IServiceBase authService)
		{
			return redisClient.Get<User>( authService.GetUserUrn());
		}

		public static T GetUserData<T>(this IRedisClient redisClient, IServiceBase authService, string key)
		{
			return redisClient.Get<T>( authService.GetUserDataUrn<T>(key));
		}

		public static bool SaveUser(this IRedisClient redisClient, IServiceBase authService, User user, TimeSpan expiresIn)
		{
			return redisClient.Set<User>( authService.GetUserUrn(), user,expiresIn );		
		}


		public static bool SaveUserData<T>(this IRedisClient redisClient, IServiceBase authService, T data, string key, TimeSpan expiresIn)
		{
			return redisClient.Set<T>( authService.GetUserDataUrn<T>(key), data,expiresIn );		
		}

		public static void Execute(this IRedisClientsManager redis, Action<IRedisClient> commands){

			using (var client = redis.GetClient()){
				commands(client);
			}
		}

		public static T Execute<T>(this IRedisClientsManager redis, Func<IRedisClient,T> commands){
			
			using (var client = redis.GetClient()){
				return commands(client);
			}
		}

	}

}