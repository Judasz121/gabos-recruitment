using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;

namespace Gabos_recruitmentTest_web_app.Models
{
	public class CSharpTaskOneViewModel
	{
		public string zusResponse { get; set; }

		public string dateIssued { get; set; }
		public string inabilityToWorkFromDate { get; set; }
		public string inabilityToWorkToDate { get; set; }
		public string inHospitalFromDate { get; set; }
		public string inHospitalToDate { get; set; }
		public IEnumerable<ZLAdoc> ZLAdocs { get; set; }
	}

	public class CSharpTaskTwoViewModel
	{
		public IEnumerable<EconomicEntity> gusResponse { get; set; }
		[Display(Name = "NIP(y)")]
		public string searchParameters { get; set; }
	}

	public class ZLAdoc
	{
		public int Id { get; set; }

		[Display(Name = "Niezdolność do pracy od")]
		//[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
		[DataType(DataType.Date)]
		public DateTime inabilityToWorkFrom { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Niezdolność do pracy do")]
		public DateTime inabilityToWorkTo { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Pobyt w szpitalu od")]
		public DateTime? inHospitalFrom { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Pobyt w szpitalu do")]
		public DateTime? inHospitalTo { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Rodzaj zaświadczenia")]
		public string typeOfDoc { get; set; }
	}
	public class XOPresponse
	{
		public IDictionary<string, string> header { get; set; } = new Dictionary<string, string>();
		public XDocument message { get; set; }
	}

	public class EconomicEntity
	{
		public string REGON { get; set; }
		public string NIP { get; set; }
		public string NIPstatus { get; set; }
		public string Name { get; set; }
		public string Province { get; set; }
		public string District { get; set; }
		public string Commune { get; set; }
		public string City { get; set; }
		public string PostCode { get; set; }
		public string Street { get; set; }
		public string RealEstateNum { get; set; }
		public string ApartmentNum { get; set; }
		public string Type { get; set; }
		public string SilosId { get; set; }

		[DataType(DataType.Date)]
		public DateTime? EconomicActivityEnd { get; set; }

	}
}
