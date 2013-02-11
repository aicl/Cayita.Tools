using System;
using System.Collections.Generic;

namespace Cayita.Tools
{
	public class AuthResponse<T>
	{
		public AuthResponse(){
			Result= new List<T>();
		}
		
		public AuthResponse(T data){
			Result= new List<T>();
			Result.Add( data );
		}
		
		public List<T> Result {get;set;}
		public string Html {get;set;}
		
		public long? TotalCount {get;set;}
	}
}

