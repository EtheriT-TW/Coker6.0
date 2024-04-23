using EtheriT.Coker.Application.Shared.Dto.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.Contact
{
	public class FormSubmitDto
	{
		public string RouterName {  get; set; }
		public MailUserDataDto Sender { get; set; }
		public List<FormFieldDto> forms { get; set; }
	}
}
