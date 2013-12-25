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

			OrmLiteConfig.DialectProvider = MySqlDialect.Provider;

			var dbFactory = new OrmLiteConnectionFactory(strCon);
									
			OrmLiteAuthRepository authRepo = new OrmLiteAuthRepository(	dbFactory);

			AuthRepoProxy rp = new AuthRepoProxy(dbFactory, null);

			rp.CreateAuthTables(authRepo,false);

			rp.SetEngine (authRepo);

			string password = rp.CreateRandomPassword(8);

			tmp = password;
			Console.WriteLine("Digite la clave para {0} [{1}] Enter para continuar", "admin", password);
			password= Console.ReadLine();
			if(password.IsNullOrEmpty()) password=tmp;

			password= rp.CreateAdminUser(authRepo, "Admin", "App", "admin@gmail.com", password);


			//

			password = rp.CreateRandomPassword(8);
			
			tmp = password;
			Console.WriteLine("Digite la clave para {0} [{1}] Enter para continuar", "user", password);
			password= Console.ReadLine();
			if(password.IsNullOrEmpty()) password=tmp;
			UserAuth user = new UserAuth{ UserName="user", Email="user@mail.com"};

			password= rp.CreateUser (authRepo, user,password);
			Console.WriteLine(password);


			dbFactory.Run (c => {
				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Radicar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Radicar", Title = "Recepcionista"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Asignar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Asignar", Title = "Secretario General"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Solucionar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Solucionar", Title = "Abogado"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Firmar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Firmar", Title = "Abogado"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="RegistrarFirmar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "RegistrarFirmar", Title = "Recepcionista"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Alistar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Alistar", Title = "Recepcionista"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Entregar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Entregar", Title = "Mensajero"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Cerrar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Cerrar", Title = "Recepcionista"});

				if(c.FirstOrDefault<AuthRole>(f=>f.Name=="Consultar")==default(AuthRole))
					c.Insert<AuthRole>(new AuthRole{Name = "Consultar", Title = "Público"});


			});

		}


	}
}
