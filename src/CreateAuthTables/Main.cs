using System;
using ServiceStack.Common;
using ServiceStack.ServiceInterface.Auth;
using Cayita.Tools.Auth;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;

namespace CreateAuthTables
{
	class MainClass
	{

		public static void Main (string[] args)
		{
			var strCon = ConfigUtils.GetConnectionString("ApplicationDb");

			string tmp = strCon;
			Console.WriteLine("Digite la cadena de conexion  [{0}] Enter para continuar", strCon);
			strCon= Console.ReadLine();
			if(strCon.IsNullOrEmpty()) strCon=tmp;

			OrmLiteConfig.DialectProvider = FirebirdDialect.Provider;

			var dbFactory = new OrmLiteConnectionFactory(strCon);
									
			OrmLiteAuthRepository authRepo = new OrmLiteAuthRepository(	dbFactory);

			AuthRepoProxy rp = new AuthRepoProxy(dbFactory, null);

			rp.CreateAuthTables(authRepo,false);

			string password = rp.CreateRandomPassword(8);

			tmp = password;
			Console.WriteLine("Digite la clave para {0} [{1}] Enter para continuar", "admin", password);
			password= Console.ReadLine();
			if(password.IsNullOrEmpty()) password=tmp;

			password= rp.CreateAdminUser(authRepo, "Admin", "App", "admin@gmail.com", password);

			Console.WriteLine(password);

		}


	}
}
