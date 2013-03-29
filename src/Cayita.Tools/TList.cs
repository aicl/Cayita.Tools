using System.Collections.Generic;

namespace Cayita.Tools
{

	public class TList<T>
	{
		long ? totalCount;
		
		public TList(){
			Result= new List<T>();
		}
				
		public List<T> Result{
			get;set;
		}

		public string Html {get;set;}
		
		public long? TotalCount {
			get { return totalCount.HasValue ? totalCount.Value : Result.Count;}
			set { totalCount = value;}
		}
	}

}

