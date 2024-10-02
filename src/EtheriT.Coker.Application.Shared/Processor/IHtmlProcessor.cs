using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Processor
{
	public interface IHtmlProcessor
	{
		public string RemoveNode(string html,string selector);
		public List<string> find(string html, string selector);
		public string text(string html);
	}
}
