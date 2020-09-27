using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Gabos_recruitmentTest_web_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gabos_recruitmentTest_web_app.Controllers
{
	public class GusWebServiceController : Controller
	{
		const string gusWebServiceAddress = "https://wyszukiwarkaregontest.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc";
		const string userKey = "abcde12345abcde12345";
		private string SId = "";


		public void Zaloguj()
		{
			string message = @"<?xml version=""1.0"" encoding=""utf-8""?>
	<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns=""http://CIS/BIR/PUBL/2014/07"">
		<soap:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
			<wsa:To>https://wyszukiwarkaregontest.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc</wsa:To>
			<wsa:Action>http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/Zaloguj</wsa:Action>
		</soap:Header>
			<soap:Body>
			<ns:Zaloguj>
				<ns:pKluczUzytkownika>" + userKey + @"</ns:pKluczUzytkownika>
			</ns:Zaloguj>
		</soap:Body>
	</soap:Envelope>";
			XOPresponse response = SendMessage(message, "");
			SId = GetZalogujResponseData(response);
		}

		public IEnumerable<EconomicEntity> DaneSzukajPodmioty(ICollection<string> NIPs)
		{
			string xmlNIPlist = "";
			if (NIPs.Count == 1)
			{
				xmlNIPlist = "<dat:Nip>" + NIPs.FirstOrDefault() + "</dat:Nip>";
			}
			else if (NIPs.Count > 1)
			{
				xmlNIPlist = "<dat:Nipy>";
				int i = 1;
				foreach (string NIP in NIPs)
				{
					if(i == NIPs.Count)
						xmlNIPlist += NIP + "</dat:Nipy>";
					else
						xmlNIPlist += NIP + ",";
					i++;
				}
			}
			else return null;
			string message = @"<?xml version=""1.0"" encoding=""utf-8""?>
	<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:ns=""http://CIS/BIR/PUBL/2014/07"" xmlns:dat=""http://CIS/BIR/PUBL/2014/07/DataContract"">
		<soap:Header xmlns:wsa=""http://www.w3.org/2005/08/addressing"">
			<wsa:To>https://wyszukiwarkaregontest.stat.gov.pl/wsBIR/UslugaBIRzewnPubl.svc</wsa:To>
			<wsa:Action>http://CIS/BIR/PUBL/2014/07/IUslugaBIRzewnPubl/DaneSzukajPodmioty</wsa:Action>
		</soap:Header>
		<soap:Body>
			<ns:DaneSzukajPodmioty >
				<ns:pParametryWyszukiwania>
					  " + xmlNIPlist + @"
				</ns:pParametryWyszukiwania>		
			</ns:DaneSzukajPodmioty>		 
		</soap:Body>
	</soap:Envelope>";
			XOPresponse response = SendMessage(message, "");
			return GetDaneSzukajPodmiotyResponseData(response);
		}

		public XOPresponse SendMessage(string msg, string action)
		{
			XDocument message = XDocument.Parse(msg);
			HttpWebRequest webRequest = CreateWebRequest(action);
			using (Stream stream = webRequest.GetRequestStream())
			{
				message.Save(stream);
			}

			IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			string response;
			using (WebResponse WR = webRequest.EndGetResponse(asyncResult))
			{
				using (StreamReader SR = new StreamReader(WR.GetResponseStream()))
				{
					response = SR.ReadToEnd();
				}
			}
			XOPresponse xopResult = DeserializeXOPresponse(response);
			return xopResult;
		}

		private HttpWebRequest CreateWebRequest(string action)
		{
			HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(gusWebServiceAddress);
			if(action != "")
				WR.Headers.Add("SOAPAction", action);
			if (SId != "")
				WR.Headers.Add("sid", SId);
			WR.ContentType = "application/soap+xml; charset=\"utf-8\"";
			WR.Accept = "application/xop+xml";
			WR.Method = "POST";
			return WR;
		}

		public string ConvertXmlToString(XNode xml)
		{
			StringBuilder builder = new StringBuilder();
			using (StringWriter SW = new StringWriter(builder))
			{
				using (XmlTextWriter XTW = new XmlTextWriter(SW))
				{
					XTW.Formatting = Formatting.Indented;
					xml.WriteTo(XTW);
				}
			}
			return HttpUtility.HtmlDecode(builder.ToString());
		}
		public XOPresponse DeserializeXOPresponse(string response)
		{
			XOPresponse resultObj = new XOPresponse();

			response = Regex.Replace(response, "--uuid:.*", "");
			string[] splitResponse = Regex.Split(response, "<s:Envelope");
			splitResponse[1] = "<s:Envelope" + splitResponse[1];
			string header = splitResponse[0];
			XDocument xmlMessage = XDocument.Parse(splitResponse[1]);

			resultObj.message = xmlMessage;

			string[] headerOptions = header.Split(Environment.NewLine, StringSplitOptions.None);
			foreach(string HO in headerOptions)
			{
				if (HO != "")
				{
					string[] splitHO = Regex.Split(HO, ": ");
					splitHO[0] = splitHO[0].Replace("\n", "");
					splitHO[1] = splitHO[1].Replace("\n", "");

					resultObj.header.Add(splitHO[0], splitHO[1]);
				}
			}

			return resultObj;
		}

		public string GetZalogujResponseData(XOPresponse response)
		{
			XDocument doc = response.message;
			string result = "";

			result = doc.Descendants().Single(predicate => predicate.Name.LocalName == "ZalogujResult").Value;
			return result;
		}
		public ICollection<EconomicEntity> GetDaneSzukajPodmiotyResponseData(XOPresponse response)
		{
			ICollection<EconomicEntity> result = new List<EconomicEntity>();

			string CDATA = response.message.Descendants().Single(p => p.Name.LocalName == "DaneSzukajPodmiotyResult").Value;
			IEnumerable<XElement> dataNodes = XDocument.Parse(CDATA).Descendants("dane");
			foreach(XElement dataNode in dataNodes)
			{
				EconomicEntity EE = new EconomicEntity()
				{
					REGON = dataNode.Descendants().Single(p => p.Name.LocalName == "Regon").Value,
					NIP = dataNode.Descendants().Single(p => p.Name.LocalName == "Nip").Value,
					NIPstatus = dataNode.Descendants().Single(p => p.Name.LocalName == "StatusNip").Value,
					Name = dataNode.Descendants().Single(p => p.Name.LocalName == "Nazwa").Value,
					Province = dataNode.Descendants().Single(p => p.Name.LocalName == "Wojewodztwo").Value,
					District = dataNode.Descendants().Single(p => p.Name.LocalName == "Powiat").Value,
					Commune = dataNode.Descendants().Single(p => p.Name.LocalName == "Gmina").Value,
					City = dataNode.Descendants().Single(p => p.Name.LocalName == "Miejscowosc").Value,
					PostCode = dataNode.Descendants().Single(p => p.Name.LocalName == "KodPocztowy").Value,
					Street = dataNode.Descendants().Single(p => p.Name.LocalName == "Ulica").Value,
					RealEstateNum = dataNode.Descendants().Single(p => p.Name.LocalName == "NrNieruchomosci").Value,
					ApartmentNum = dataNode.Descendants().Single(p => p.Name.LocalName == "NrLokalu").Value,
					Type = dataNode.Descendants().Single(p => p.Name.LocalName == "Typ").Value,
					SilosId = dataNode.Descendants().Single(p => p.Name.LocalName == "SilosID").Value,
				};
				string date = dataNode.Descendants().Single(p => p.Name.LocalName == "DataZakonczeniaDzialalnosci").Value;
				if(date != "")
					EE.EconomicActivityEnd = DateTime.Parse(date);

				result.Add(EE);
			}
			return result;
		}





	}
}
