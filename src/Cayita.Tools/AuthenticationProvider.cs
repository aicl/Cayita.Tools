using System;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.Redis;
using ServiceStack.Common.Extensions;
using System.Globalization;
using ServiceStack.FluentValidation.Results;

namespace Cayita.Tools.Auth
{
	public class AuthenticationProvider:CredentialsAuthProvider
	{
		
		class CredentialsAuthValidator : AbstractValidator<ServiceStack.ServiceInterface.Auth.Auth>
		{
			public CredentialsAuthValidator()
			{
				RuleFor(x => x.UserName).NotEmpty();
				RuleFor(x => x.Password).NotEmpty();
			}
		}
		
		public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
		{
			if (authService == null)
				throw new ArgumentNullException ("authService");
			var authRepo = authService.TryResolve<IUserAuthRepository>();
			if (authRepo == null)
			{
				Log.WarnFormat("Tried to authenticate without a registered IUserAuthRepository");
				return false;
			}
			var session = authService.GetSession();
			UserAuth userAuth = null;
			if (authRepo.TryAuthenticate(userName, password, out userAuth))
			{

				var usermeta =userAuth.Get<UserMeta>();
				if(usermeta!=default(UserMeta))
				{

					if(!usermeta.IsActive)
						throw HttpError.Unauthorized("User Inactive");

					if(usermeta.ExpiresAt.HasValue && usermeta.ExpiresAt.Value<DateTime.Now)
						throw HttpError.Unauthorized("account has expired");
				}

				var cache = authService.TryResolve<IRedisClientsManager>();
				if(cache!=null)
				{
					User user = new User();
					user.PopulateWith(userAuth);
					user.IsActive=usermeta!=null? usermeta.IsActive:true;
					user.Info=  usermeta!=null?usermeta.Info:"";
					user.ExpiresAt= usermeta!=null?usermeta.ExpiresAt:null;

					cache.Execute(client=>{
						client.Set<User>( authService.GetUserUrn(), user, SessionExpiry.Value );
					});


				}

				session.PopulateWith(userAuth);
				session.IsAuthenticated = true;
				session.UserAuthId =  userAuth.Id.ToString(CultureInfo.InvariantCulture);

				//session.ProviderOAuthAccess = authRepo.GetUserOAuthProviders(session.UserAuthId)
				//								.ConvertAll(x => (IOAuthTokens)x);
				//userAuth.Meta fecha de expiracion , y otros datos!!!
				return true;
			}
			return false;
		}
		
		/*
		public override void OnAuthenticated (ServiceStack.ServiceInterface.IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
		{		
			base.OnAuthenticated (authService, session, tokens, authInfo);
		}
		*/
		
		public override object Authenticate(IServiceBase authService, IAuthSession session, 
		                                    ServiceStack.ServiceInterface.Auth.Auth request)
		{
			new CredentialsAuthValidator().ValidateAndThrow(request);
			return CustomAuthenticate(authService, session, request.UserName, request.Password);
		}
		
		protected object CustomAuthenticate(IServiceBase authService, IAuthSession session, string userName, string password)
		{
			if (!LoginMatchesSession(session, userName))
			{
				authService.RemoveSession();
				session = authService.GetSession();
			}
			
			if (TryAuthenticate(authService, userName, password))
			{
				if (session.UserAuthName == null)
					session.UserAuthName = userName;

				OnAuthenticated(authService, session, null, null);
				return new AuthResponse {
					UserName = userName,
					SessionId = session.Id,
				};
			}

			//throw HttpError.Unauthorized("user/password invalid");
			var vf = new ValidationFailure("Unauthorized","user/password invalid","Unauthorized");
			throw new ValidationException( new ValidationFailure[]{vf} );
		}
		/*
		public override bool IsAuthorized(IAuthSession session, IOAuthTokens tokens, Auth request=null)
		{
			return base.IsAuthorized(session, tokens, request);
		}
		*/
	}

}

