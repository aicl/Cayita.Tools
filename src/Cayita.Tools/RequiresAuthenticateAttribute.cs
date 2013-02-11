using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Redis;

namespace Cayita.Tools.Auth
{
	public class RequiresAuthenticateAttribute:AuthenticateAttribute
	{
		
		public RequiresAuthenticateAttribute(ApplyTo applyTo)
		: base(applyTo)	{}
		
		public RequiresAuthenticateAttribute()
		: base(ApplyTo.All) {}
		
		public RequiresAuthenticateAttribute(string provider)
		: this(ApplyTo.All)	{}
		
		public RequiresAuthenticateAttribute(ApplyTo applyTo, string provider)
		: this(applyTo)	{}
		
		
		public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
		{
			base.Execute(req, res, requestDto);
			var session = req.GetSession();
			if(session!=null && session.IsAuthenticated)
			{
				req.SaveSession(session);// refresh session TTL
				var cache = req.TryResolve<IRedisClientsManager>();
				if(cache!=null){
					using(var client = cache.GetClient()){
						var pattern = string.Format("urn:{0}:*", req.GetSessionId());
						var keys =client.SearchKeys(pattern);
						foreach(var k in keys) {
							client.ExpireEntryIn(k, AuthProvider.DefaultSessionExpiry);
						}
					}
				}
			}
		}
		
	}
}

