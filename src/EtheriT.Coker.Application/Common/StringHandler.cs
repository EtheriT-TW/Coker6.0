using AutoMapper.Execution;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EtheriT.Coker.Application.Common
{
    public class StringHandler
    {
        public string HtmlDecode(string? html) {
            string output;
            if (html == null) output = "";
            else
            {
                output = HttpUtility.HtmlDecode(html);
                if (output.IndexOf("&amp;") >= 0)
                {
                    output = HtmlDecode(output);
                }
            }
            return output;
        }
        public string HtmlEncode(string? html)
        {
            string output = HtmlDecode(html);
            return HttpUtility.HtmlEncode(output);
        }
        public string privacyName(string Name) {
            return Name.Substring(0, 1) + "○" + Name.Substring(Name.Length - 1);
        }
        public string RandonCode(RandomStringType type,int length ) { 
            string str = string.Empty;
            switch (type) {
                case RandomStringType.數字:
                    str = RandonNumCode(length);
					break;
				case RandomStringType.英文小寫:
					str = RandonWordCode(length).ToLower();
					break;
				case RandomStringType.英文大寫:
					str = RandonWordCode(length).ToUpper();
					break;
                case RandomStringType.數字加英文小寫:
					Random random = new Random();
                    for (int i=0; i<length ; i++) { 
                        int c = random.Next(0, 1);
                        switch (c) { 
                            case 0:
                                str += RandonNumCode(1);
								break;
                            case 1:
								str += RandonWordCode(1).ToLower();
								break;
                        }
					}
					break;
				case RandomStringType.數字加英文大小寫:
					Random random2 = new Random();
					for (int i = 0; i < length; i++)
					{
						int c = random2.Next(0, 2);
						switch (c)
						{
							case 0:
								str += RandonNumCode(1);
								break;
							case 1:
								str += RandonWordCode(1).ToLower();
								break;
							case 2:
								str += RandonWordCode(1).ToUpper();
								break;
						}
					}
					break;
				case RandomStringType.數字加英文大小寫及符號:
					Random random3 = new Random();
					for (int i = 0; i < length; i++)
					{
						int c = random3.Next(0, 3);
						switch (c)
						{
							case 0:
								str += RandonNumCode(1);
								break;
							case 1:
								str += RandonWordCode(1).ToLower();
								break;
							case 2:
								str += RandonWordCode(1).ToUpper();
								break;
							case 3:
								str += RandonPunctuationCode(1);
								break;
						}
					}
					break;
			}
            return str;
        }
        private string RandonNumCode(int length) { 
            return (new Random()).Next((int) Math.Pow(10, length)-1).ToString();
        }
		private string RandonWordCode(int length)
		{
			Random random = new Random();
			string str = string.Empty;
            for (int i = 0;i<length ;i++) {
				str += (char)('A' + (char)(random.Next() % 26));
			}
			return str;
		}
		private string RandonPunctuationCode(int length)
		{
			string PunctuationList = @"~!@#$%^&*()_+{}|:""<>?-=,./;'[]";
			Random random = new Random();
			string str = string.Empty;
			for (int i = 0; i < length; i++)
			{
				str += PunctuationList[random.Next(0, PunctuationList.Length)];
			}
			return str;
		}
	}
}
