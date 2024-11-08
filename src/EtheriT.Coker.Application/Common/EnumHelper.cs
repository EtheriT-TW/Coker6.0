using EtheriT.Coker.Application.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Common
{
	public class EnumHelper
	{
		public static List<SelectDto> EnumToKeyValueList<T>() where T : Enum
		{
			return Enum.GetValues(typeof(T))
					   .Cast<T>()
					   .Select(e => new SelectDto{Id = Convert.ToInt32(e), Name = e.ToString()})
					   .ToList();
		}
	}
}
