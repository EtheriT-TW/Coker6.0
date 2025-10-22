using AutoMapper.Execution;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
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
                    for (int i=0; i<length ; i++) {
                        int c = RandomNum(max: 1);
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
					for (int i = 0; i < length; i++)
					{
						int c = RandomNum(0, 2);
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
					for (int i = 0; i < length; i++)
					{
						int c = RandomNum(0, 3);
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
            if (length <= 0)
                throw new ArgumentException("Length must be a positive integer.");

            // 使用 RandomNumberGenerator 生成隨機數字
            byte[] randomNumber = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            // 將隨機數字轉換為字串
            string randomNumStr = string.Empty;
            foreach (byte b in randomNumber)
            {
                // 轉換為 0-9 的數字
                randomNumStr += (b % 10).ToString();
            }

            // 確保結果的長度符合要求
            return randomNumStr.Substring(0, length);
        }
        public int RandomNum(int min = 0, int max = int.MaxValue)
        {
            if (min >= max)
                throw new ArgumentException("Max must be greater than Min.");

            // 使用 RandomNumberGenerator 生成隨機整數
            byte[] randomNumber = new byte[4]; // 4 bytes for an integer
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            // 轉換為無符號整數
            uint randomValue = BitConverter.ToUInt32(randomNumber, 0);
            // 返回介於 min 和 max 之間的整數
            return (int)(randomValue % (max - min)) + min;
        }
        private string RandonWordCode(int length)
		{
			string str = string.Empty;
            for (int i = 0;i<length ;i++) {
				str += (char)('A' + (char)(int.Parse(RandonNumCode(2)) % 26));
			}
			return str;
		}
		private string RandonPunctuationCode(int length)
		{
			string PunctuationList = @"~!@#$%^&*()_+{}|:""<>?-=,./;'[]";
			string str = string.Empty;
			for (int i = 0; i < length; i++)
			{
				str += PunctuationList[RandomNum(0, PunctuationList.Length)];
			}
			return str;
		}
        public string RemoveQueryParam(string rawUrl, params string[] keysToRemove)
        {
            var uri = new Uri(Uri.UnescapeDataString(rawUrl));
            var queryParams = QueryHelpers.ParseQuery(uri.Query);
            var cleaned = queryParams
                .Where(kv => !keysToRemove.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());

            return QueryHelpers.AddQueryString(uri.GetLeftPart(UriPartial.Path), cleaned);
        }
        public List<long> ParseCsvIds(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<long>();
            var list = new List<long>();
            foreach (var token in csv.Split(',', StringSplitOptions.RemoveEmptyEntries))
                if (long.TryParse(token.Trim(), out var id)) list.Add(id);
            return list;
        }
    }
}
