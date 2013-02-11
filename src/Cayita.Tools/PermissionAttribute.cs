using System.Net;
﻿using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

namespace Cayita.Tools
{
	public class PermissionAttribute:RequiredPermissionAttribute
	{
		public PermissionAttribute(params string[] permissions):base(ApplyTo.All, permissions)
		{
		}
		public PermissionAttribute(ApplyTo applyTo, params string[] permissions):base(applyTo, permissions)
		{
		}
		
		public override void Execute (IHttpRequest req, IHttpResponse res, object requestDto)
		{
			AuthenticateAttribute.AuthenticateIfBasicAuth(req, res);
			
			var session = req.GetSession();
			
			if(session!=null)
			{
				if ( HasAllPermissions(session)) return;
			}
			
			res.StatusCode = (int)HttpStatusCode.Unauthorized;
			res.StatusDescription = "Invalid Permissions";
			res.Close();
		}
		
	}
}

