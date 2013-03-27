using System;
using ServiceStack.Common;
using ServiceStack.OrmLite;
using ServiceStack.Redis;
using System.Data;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using System.Collections.Generic;

namespace Cayita.Tools.Auth
{
	public class AuthRepoProxy
	{

		IDbConnectionFactory DbConnectionFactory {get;set;}
		
		IRedisClientsManager RedisClientsManager {get;set;}
		
		public AuthRepoProxy (IDbConnectionFactory dbConnectionFactory, IRedisClientsManager redisClientsManager)
		{
			DbConnectionFactory= dbConnectionFactory;
			RedisClientsManager= redisClientsManager;		}
		

		public void Execute( Action<IDbConnection> commands)
		{
			DbConnectionFactory.Run(dbCmd=>{commands(dbCmd);});
		}
		
		public T Execute<T>( Func<IDbConnection,T> commands)
		{
			return DbConnectionFactory.Run(dbCmd=>{return commands(dbCmd);});
		}

		public void CreateAuthTables(OrmLiteAuthRepository authRepo, bool overwrite=false){

			string engine = "ENGINE = InnoDB";

			authRepo.CreateMissingTables();

			DbConnectionFactory.Run(db=>{

				db.CreateTable<AuthPermission>(overwrite);
				db.CreateTable<AuthRole>(overwrite);
				db.CreateTable<AuthRolePermission>(overwrite);
				db.CreateTable<AuthRoleUser>(overwrite);
				db.CreateTable<RolePermission>(overwrite);
				db.CreateTable<UserRole>(overwrite);

				db.AlterTable<UserAuth>(engine);
				db.AlterTable<UserOAuthProvider>(engine);

				db.AlterTable<AuthPermission>(engine);
				db.AlterTable<AuthRole>(engine);
				db.AlterTable<AuthRolePermission>(engine);
				db.AlterTable<AuthRoleUser>(engine);
				db.AlterTable<RolePermission>(engine);
				db.AlterTable<UserRole>(engine);

			});
		}


		public string CreateAdminUser(OrmLiteAuthRepository authRepo,
		                              string firstname, string lastname, string email,
		                              string password )
		{
			if(password.IsNullOrEmpty()) password= CreateRandomPassword(8);

			if(firstname.IsNullOrEmpty()) firstname="Admin";
			if(lastname.IsNullOrEmpty()) lastname="App";
			if(email.IsNullOrEmpty()) email= "admin@no-mail.com";
			string displayname = "{0} {1}".Fmt(firstname, lastname);
			string userName = "admin";

			var newUser= new UserAuth {
				DisplayName = displayname,
				Email = email,
				UserName = userName,
				FirstName = firstname,
				LastName = lastname,
				ModifiedDate= DateTime.Now
			};

			var userAuth=authRepo.GetUserAuthByUserName(userName);

			if ( userAuth== default(UserAuth) )
			{
				newUser.Permissions=new List<string>(new string[]{});
				newUser.Roles =new List<string>(new string[]{RoleNames.Admin});
				newUser.Meta= new Dictionary<string,string>();
				userAuth =authRepo.CreateUserAuth(newUser,password);
			}
			else
			{
				userAuth=authRepo.UpdateUserAuth(userAuth, newUser, password);
			}

			DbConnectionFactory.Run(db=>{
				string roleName="Admin";
				var role= db.FirstOrDefault<AuthRole>(r=> r.Name==roleName);

				if(role==default(AuthRole))
				{
					role= new AuthRole(){Name=roleName, Title=roleName};
					db.InsertAndAssertId(role);
				}

				AuthRoleUser auru= db.FirstOrDefault<AuthRoleUser>(r=> r.AuthRoleId==role.Id &&
				                                                      r.UserId==userAuth.Id);
				if(auru==default(AuthRoleUser))
				{
					auru=new AuthRoleUser(){UserId=userAuth.Id, AuthRoleId= role.Id};
					db.Insert(auru);
				}

				roleName="Users";

				role= db.FirstOrDefault<AuthRole>(q=>q.Name==roleName);
				if(role==default(AuthRole)){
					role = new AuthRole {
						Name=roleName,
						Directory="user",
						Title="User's Managment",
						ShowOrder="99"
					};
					db.InsertAndAssertId(role);

				}

				auru= db.FirstOrDefault<AuthRoleUser>(r=> r.AuthRoleId==role.Id &&
				                                     r.UserId==userAuth.Id);
				
				if(auru==default(AuthRoleUser)){
					auru= new AuthRoleUser{
						AuthRoleId=role.Id,
						UserId= userAuth.Id
					};
					db.Insert(auru);
				};

			});

			return password;

		}


		public string CreateUser(OrmLiteAuthRepository authRepo, string userName, string email,
		                              string firstname=null, string lastname=null, 
		                              string password=null )
		{
			if(password.IsNullOrEmpty()) password= CreateRandomPassword(8);
			
			if(firstname.IsNullOrEmpty()) firstname="User";
			if(lastname.IsNullOrEmpty()) lastname="App";
			string displayname = "{0} {1}".Fmt(firstname, lastname);
						
			var newUser= new UserAuth {
				DisplayName = displayname,
				Email = email,
				UserName = userName,
				FirstName = firstname,
				LastName = lastname,
				ModifiedDate= DateTime.Now
			};
			
			var userAuth=authRepo.GetUserAuthByUserName(userName);
			
			if ( userAuth== default(UserAuth) )
			{
				newUser.Permissions=new List<string>(new string[]{});
				newUser.Roles =new List<string>(new string[]{});
				newUser.Meta= new Dictionary<string,string>();
				userAuth =authRepo.CreateUserAuth(newUser,password);
			}
			else
			{
				userAuth=authRepo.UpdateUserAuth(userAuth, newUser, password);
			}

			return password;
			
		}


		public string CreateRandomPassword(int passwordLength) 
		{ 
			string allowedChars = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@$?"; 
			Byte[] randomBytes = new Byte[passwordLength]; 
			char[] chars = new char[passwordLength]; 
			int allowedCharCount = allowedChars.Length; 
			
			for(int i = 0;i<passwordLength;i++) 
			{ 
				Random randomObj = new Random(); 
				randomObj.NextBytes(randomBytes); 
				chars[i] = allowedChars[(int)randomBytes[i] % allowedCharCount]; 
			} 
			
			return new string(chars); 
		}

	}
}

