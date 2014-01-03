using System;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Cayita.Tools
{
	public class Mailer
	{
		SmtpClient SmtpServer {get ;set;}

		public string Server {private get; set;}
		public string User { get; set;}
		public int Port {private get; set;}
		public string Password {private get; set;}
		public bool EnableSsl {private get; set;}

		public  Mailer(){}

		public Mailer ( string server, int port, string user, string password,bool enableSsl=true)
		{

			Server=server;
			User= user;
			Password=password;
			EnableSsl= enableSsl;
			Port= port;
			Authenticate();
		}

		private void Authenticate()
		{
	
			SmtpServer = new SmtpClient(Server,Port);
			SmtpServer.Credentials = 	new NetworkCredential(User, Password);
			SmtpServer.EnableSsl = EnableSsl;

			ServicePointManager.ServerCertificateValidationCallback =
				delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
					{ return true; };
		}

	
		public void Send(Action<MailMessage> mail ){

			if(SmtpServer== null) Authenticate();

			using (var message = new MailMessage())
			{
				message.From= new MailAddress(User, User); 
				mail(message);
				SmtpServer.Send(message);
			}

		}

		public void Reset()
		{
			if( SmtpServer!=null){
				SmtpServer.Dispose();
				SmtpServer=null;
			}
		}
	}
}
