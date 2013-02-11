using System;
using System.Linq;
using ServiceStack.Common;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;
using ServiceStack.OrmLite;
using Mono.Linq.Expressions;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface.Auth;
using System.Collections.Generic;

namespace Cayita.Tools.Auth
{
	public class UserManager
	{
		IRequestContext RequestContext{get;set;}
		AuthRepoProxy AuthRepoProxy {get;set;}

		public UserManager (IRequestContext requestContex, AuthRepoProxy authRepoProxy)
		{
			RequestContext= requestContex;
			AuthRepoProxy = authRepoProxy;
		}

		public TList<User> GetUser(GetUser request){

			var httpRequest = RequestContext.Get<IHttpRequest>();
					
			var userSession = httpRequest.GetSession();
							
			long? totalCount=null;
				
			var pager= httpRequest.BuildPager();
				
			var visitor = ReadExtensions.CreateExpression<User>();
			var predicate = PredicateBuilder.True<User>();
				
			if (userSession.HasRole(RoleNames.Admin))
			{
				if(request.Id!=default(int))
					predicate= q=>q.Id==request.Id;

				if(!request.UserName.IsNullOrEmpty()) 
					predicate= q=>q.UserName.StartsWith(request.UserName) ;

				if(userSession.UserName.ToLower()!=RoleNames.Admin.ToLower())
					predicate=predicate.AndAlso(q=>q.UserName!=RoleNames.Admin);
					
			}
			else
			{
				var id = int.Parse(userSession.UserAuthId);
				predicate= q=>q.Id==id;
			}
				
			visitor.Where(predicate).OrderBy(r=>r.UserName);


			return AuthRepoProxy.Execute(db=>{

				if(pager.PageNumber.HasValue)
				{
					totalCount=  db.Count(predicate);								
					int rows= pager.PageSize.HasValue? pager.PageSize.Value:Defs.PageSize;
					visitor.Limit(pager.PageNumber.Value*rows, rows);
				}
				
				return new TList<User>(){
					TotalCount=totalCount,
					Result=db.Select(visitor)
				};

			});
		
		}


		public User CreateUser(CreateUser request)
		{
			var httpRequest = RequestContext.Get<IHttpRequest>();
			
			var authRepo = httpRequest.TryResolve<IUserAuthRepository>();
			if(authRepo==null)
				throw HttpError.NotFound("AuthRepository NO found");
			
			var  user= new UserAuth
			{	
				FirstName= request.FirstName,
				LastName= request.LastName,
				Email= request.Email,
				UserName= request.UserName,
				DisplayName = request.FirstName +" "+ request.LastName
			};
			user.Set<UserMeta>( new UserMeta{
				Info= request.Info,
				IsActive=request.IsActive,
				ExpiresAt= request.ExpiresAt
			});
			
			user = authRepo.CreateUserAuth(user, request.Password);

			User u = new User();
			u.PopulateWith(user);
			return u;
		}


		public User Update(UpdateUser request)
		{
			var httpRequest = RequestContext.Get<IHttpRequest>();

			var userSession = httpRequest.GetSession();
			
			if(!( userSession.HasRole(RoleNames.Admin) 
			     ||     userSession.HasPermission("User.update") 
			 ))
				throw HttpError.Unauthorized("Update no allowed");
			
			
			var authRepo = httpRequest.TryResolve<IUserAuthRepository>();
			if(authRepo==null)
				throw HttpError.NotFound("AuthRepository NO found");
			
			var  user= authRepo.GetUserAuth(request.Id.ToString());
			
			if (!(request.Id== int.Parse(userSession.UserAuthId) ||
			      userSession.HasRole(RoleNames.Admin)) )
				throw HttpError.Unauthorized("Update no allowed (no admin)");
			
			if(user == default(UserAuth))
				throw HttpError.NotFound(
					string.Format("User  Id:'{0}' NO found",request.Id));
			
			
			var  newUser= new UserAuth
			{	
				Id= request.Id,
				FirstName= request.FirstName,
				LastName= request.LastName,
				Email= request.Email,
				UserName= request.UserName,
				DisplayName= request.FirstName+" "+request.LastName,
				ModifiedDate= DateTime.Now,
			};
			newUser.Set<UserMeta>( new UserMeta{
				Info= request.Info,
				IsActive=request.IsActive,
				ExpiresAt= request.ExpiresAt
			});


			if(request.Password.IsNullOrEmpty())
			{
					AuthRepoProxy.Execute(db=>{
					
					db.UpdateOnly(
						newUser,
						ev=>ev.Where(q=>q.Id==request.Id).
						Update(f=> new {
						f.UserName, f.FirstName, f.LastName, f.Email, f.Meta,
						f.DisplayName,
						f.ModifiedDate
					}));
				});

			}
			
			else
			{
				user = authRepo.UpdateUserAuth(user, newUser,request.Password);
			}

			User u = new User();
			u.PopulateWith(newUser);
			return u;						
				
		}

		public void Delete(DeleteUser request)
		{
			AuthRepoProxy.Execute(db=>{
				db.Delete<User>(q=>q.Id==request.Id);
			});
		}

		public  AuthorizationResponse GetAuthorization( GetAuthorization request)
		{
			
			var httpRequest = RequestContext.Get<IHttpRequest>();	
			IAuthSession session = httpRequest.GetSession();
			
			if (!session.HasRole(RoleNames.Admin))
			{
				request.UserId= int.Parse(session.UserAuthId);
			}
			
			List<AuthRole> roles = new List<AuthRole>();
			List<AuthPermission> permissions= new List<AuthPermission>();
			
			List<AuthRoleUser> aur= new List<AuthRoleUser>();
			List<AuthRole> rol = new List<AuthRole>();
			List<AuthPermission> per = new List<AuthPermission>();
			List<AuthRolePermission> rol_per = new List<AuthRolePermission>();
			
			AuthRepoProxy.Execute(db=>{

				aur=  db.Select<AuthRoleUser>(q=>q.UserId==request.UserId);
				//proxy.GetByUserIdFromCache<AuthRoleUser>(request.UserId); // causa problemas .net !!! no en mono
				rol= db.GetListFromCache<AuthRole>();
				per= db.GetListFromCache<AuthPermission>();
				rol_per= db.GetListFromCache<AuthRolePermission>();
				
				foreach( var r in aur)
				{
					AuthRole ar= rol.First(x=>x.Id== r.AuthRoleId);
					roles.Add(ar);
					rol_per.Where(q=>q.AuthRoleId==ar.Id).ToList().ForEach(y=>{
						AuthPermission up=  per.First( p=> p.Id== y.AuthPermissionId);
						if( permissions.FindIndex(f=>f.Name==up.Name)<0) // .IndexOf(up) <0)
							permissions.Add(up);
					}) ;
				};    
				
			});
			
			return new AuthorizationResponse(){
				Permissions= permissions,
				Roles= roles,
			};
		}

		public TList<AuthPermission> GetAuthPermission(GetAuthPermission request)
		{
			long? totalCount=null;		
			var httpRequest = RequestContext.Get<IHttpRequest>();											
			var pager= httpRequest.BuildPager();
				
			var visitor = ReadExtensions.CreateExpression<AuthPermission>();
			var predicate = PredicateBuilder.True<AuthPermission>();
				
			if(!request.Name.IsNullOrEmpty()){
				predicate= q=>q.Name.Contains(request.Name);
			}
				
			visitor.Where(predicate);

			return AuthRepoProxy.Execute(db=>{

				if(pager.PageNumber.HasValue)
				{
					totalCount= db.Count(predicate);
					int rows= pager.PageSize.HasValue? pager.PageSize.Value:Defs.PageSize;
					visitor.Limit(pager.PageNumber.Value*rows, rows);
				}
								
				return new TList<AuthPermission>(){
					Result=db.Select(visitor),
					TotalCount=totalCount
				};

			});
		}

		public  AuthPermission CreateAuthPermission(CreateAuthPermission request)
		{
			var au = new AuthPermission{Name= request.Name};
		
			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthPermission>();
				db.InsertAndAssertId(au);
			});
			
			return au;	
		}

		public AuthPermission UpdateAuthPermission(UpdateAuthPermission request)
		{
			var au =new AuthPermission{Name= request.Name};
			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthPermission>();
				db.Update(au);
			});
			
			return 	au;
		}


		public void DeleteAuthPermission(DeleteAuthPermission request)
		{
			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthPermission>();
				db.Delete<AuthPermission>(q=>q.Id==request.Id);
			});
		
		}


		public TList<AuthRole> GetAuthRole(GetAuthRole request)
		{
			var httpRequest = RequestContext.Get<IHttpRequest>();
			long? totalCount=null;
			
			var pager= httpRequest.BuildPager();
			var visitor = ReadExtensions.CreateExpression<AuthRole>();
			var predicate = PredicateBuilder.True<AuthRole>();
						
			if(!request.Name.IsNullOrEmpty()){
				predicate= q=>q.Name.Contains(request.Name);
			}
				
			visitor.Where(predicate);

			return AuthRepoProxy.Execute(db=>{
							
				if(pager.PageNumber.HasValue)
				{
					totalCount= db.Count(predicate);
					int rows= pager.PageSize.HasValue? pager.PageSize.Value:Defs.PageSize;
					visitor.Limit(pager.PageNumber.Value*rows, rows);
				}
								
				return new TList<AuthRole>(){
					Result=db.Select(visitor),
					TotalCount=totalCount
				};
			});	
		}


		public  AuthRole CreateAuthRole(CreateAuthRole request)
		{
			var ar = new AuthRole();
			ar.PopulateWith(request);

			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthRole>();
				db.InsertAndAssertId(ar);
			});
			
			return ar;
		}

		public AuthRole UpdateAuthRole(UpdateAuthRole request)
		{
			var ar = new AuthRole();
			ar.PopulateWith(request);

			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthRole>();
				db.Update(ar);
			});
			
			return ar;	
		}

		public void DeleteAuthRole(DeleteAuthRole request)
		{
			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthRole>();
				db.Delete<AuthRole>(q=>q.Id==request.Id);
			});
				
		}



		public TList<RolePermission> GetRolePermssion(GetRolePermission request)
		{
			long? totalCount=null;
			var httpRequest = RequestContext.Get<IHttpRequest>();	
			var pager= httpRequest.BuildPager();
				
			var visitor = ReadExtensions.CreateExpression<RolePermission>();
			var predicate = PredicateBuilder.True<RolePermission>();
				
			predicate= q=>q.AuthRoleId==request.AuthRoleId;
				
			visitor.Where(predicate).OrderBy(f=>f.Name) ;

			return AuthRepoProxy.Execute(db=>{
				if(pager.PageNumber.HasValue)
				{
					totalCount= db.Count(predicate);
					int rows= pager.PageSize.HasValue? pager.PageSize.Value:Defs.PageSize;
					visitor.Limit(pager.PageNumber.Value*rows, rows);
				}

				return new TList<RolePermission>(){
					Result=db.Select(visitor),
					TotalCount=totalCount
				};
			});	
		}


		public RolePermission CreateRolePermission(CreateRolePermission request)
		{
			var rp = new RolePermission();
			rp.PopulateWith(request);

			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthRolePermission>();
				
				var permission= db.FirstOrDefault<AuthPermission>(f=>f.Id==request.AuthPermissionId);
				if(permission==default(AuthPermission))
					throw HttpError.NotFound(string.Format("No found Permission Id :'{0}'",
					                                       request.AuthPermissionId));
				
				db.InsertAndAssertId(rp);
				rp.Name= permission.Name;
			});
			
			return rp;	
		}

		public void DeleteRolePermission(DeleteRolePermission request)
		{
			AuthRepoProxy.Execute(db=>{
				//db.DeleteFromCache<AuthRolePermission>();
				db.Delete<RolePermission>(q=>q.Id==request.Id);
			});

		}


		public TList<UserRole> GetUserRole(GetUserRole request)
		{
			var httpRequest = RequestContext.Get<IHttpRequest>();
			
			var userSession = httpRequest.GetSession();
			
			long? totalCount=null;
			
			var pager= httpRequest.BuildPager();

			var visitor = ReadExtensions.CreateExpression<UserRole>();
			var predicate = PredicateBuilder.True<UserRole>();
						
			if(request.UserId==default(int))
				request.UserId= int.Parse(userSession.UserAuthId);
			else if(!(userSession.HasRole(RoleNames.Admin)) &&
			        request.UserId != int.Parse(userSession.UserAuthId)
			        )
			{
				throw HttpError.Unauthorized("User no allowed to read roles from other user");
			}
			
			predicate= q=>q.UserId==request.UserId;
			
			visitor.Where(predicate).OrderBy(f=>f.Name);

			return AuthRepoProxy.Execute(db=>{
				
				if(pager.PageNumber.HasValue)
				{
					totalCount= db.Count(predicate);
					int rows= pager.PageSize.HasValue? pager.PageSize.Value:Defs.PageSize;
					visitor.Limit(pager.PageNumber.Value*rows, rows);
				}
								
				return new TList<UserRole>(){
					Result=db.Select(visitor),
					TotalCount=totalCount
				};
			});
			
		}


		public UserRole CreateUserRole(CreateUserRole request)
		{
			var ur = new UserRole(){AuthRoleId= request.AuthRoleId, UserId=request.UserId};

			AuthRepoProxy.Execute(db=>{
				
				var role =db.FirstOrDefault<AuthRole>(f=>f.Id== request.AuthRoleId);
				if(role==default(AuthRole))
					throw HttpError.NotFound(string.Format("No found rolr  Id :'{0}'",
					                                       request.AuthRoleId));
				db.Insert(ur);
				ur.Name= role.Name;
			});
			
			return ur;
		}


		public void DeleteUserRole(DeleteUserRole request)
		{
			AuthRepoProxy.Execute(db=>{
				db.Delete<UserRole>(q=>q.Id==request.Id);
			});

		}

	}
}

