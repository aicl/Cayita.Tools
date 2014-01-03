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
using System.Linq.Expressions;
using ServiceStack.Redis;
using System.Text;
using System.Security.Cryptography;


namespace System.Collections.Generic
{
	public static partial class Extensions
	{
		public static TList<T> ConvertToTList<T>(this List<T> source, long? totalCount=null)
		{
			return  new TList<T> { Result = source, TotalCount = totalCount };
		}
	}
}

namespace ServiceStack.Text
{
	public static partial class Extensions
	{
		public static string GetQuotedFieldName<T>(this ModelDefinition modelDef, Expression<Func<T,object>> field) 
		{
			return OrmLiteConfig.DialectProvider.GetQuotedColumnName (
				modelDef.GetFieldDefinition (field).FieldName);

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
namespace ServiceStack.ServiceInterface{

	public static partial class Extensions
	{
		public static void RemoveUserData(this  IServiceBase authService)
		{
			var cache = authService.TryResolve<IRedisClientsManager>();
			if(cache!=null){
				var sessionId = authService.GetSessionId();
				
				var pattern = string.Format("urn:{0}:*", sessionId);
				cache.Execute(client=>{
					var keys =client.SearchKeys(pattern);
					client.RemoveAll(keys);
				}); 		
			}
		}
	}
}

namespace System
{
	public static partial class Extensions
	{

		public static string Encriptar(this string texto, string key)
		{
			if(string.IsNullOrEmpty(texto)) return "";

			//arreglo de bytes donde guardaremos la llave
			byte[] keyArray;
			//arreglo de bytes donde guardaremos el texto
			//que vamos a encriptar
			byte[] Arreglo_a_Cifrar = UTF8Encoding.UTF8.GetBytes(texto);
			
			//se utilizan las clases de encriptaciÃ³n
			//provistas por el Framework
			//Algoritmo MD5
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
			//se guarda la llave para que se le realice
			//hashing
			keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
			
			hashmd5.Clear();
			
			//Algoritmo 3DAS
			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
			
			tdes.Key = keyArray;
			tdes.Mode = CipherMode.ECB;
			tdes.Padding = PaddingMode.PKCS7;
			
			//se empieza con la transformaciÃ³n de la cadena
			ICryptoTransform cTransform = tdes.CreateEncryptor();
			
			//arreglo de bytes donde se guarda la
			//cadena cifrada
			byte[] ArrayResultado = cTransform.TransformFinalBlock(Arreglo_a_Cifrar,0, Arreglo_a_Cifrar.Length);
			
			tdes.Clear();
			
			//se regresa el resultado en forma de una cadena
			return Convert.ToBase64String(ArrayResultado,0, ArrayResultado.Length);
		}
		
		public static string Desencriptar(this string textoEncriptado, string key){

			if(string.IsNullOrEmpty(textoEncriptado)) return "";

			byte[] keyArray;
			//convierte el texto en una secuencia de bytes
			byte[] Array_a_Descifrar = Convert.FromBase64String(textoEncriptado);
			
			//se llama a las clases que tienen los algoritmos
			//de encriptaciÃ³n se le aplica hashing
			//algoritmo MD5
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
			
			keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
			
			hashmd5.Clear();
			
			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
			
			tdes.Key = keyArray;
			tdes.Mode = CipherMode.ECB;
			tdes.Padding = PaddingMode.PKCS7;
			
			ICryptoTransform cTransform = tdes.CreateDecryptor();
			
			byte[] resultArray = cTransform.TransformFinalBlock(Array_a_Descifrar,0, Array_a_Descifrar.Length);
			
			tdes.Clear();
			//se regresa en forma de cadena
			return UTF8Encoding.UTF8.GetString(resultArray);
		}
		
		//
		
	}
	

}
