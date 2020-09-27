using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Gabos_recruitmentTest_web_app.Controllers
{
	public class ZusWebServiceController : Controller
	{
		const string zusWebServiceAddress = "https://pue.zus.pl:8001/ws/zus.channel.gabinetoweV2:zla"; 

		public XDocument pobierzOswiadczenie()
		{
			string message = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:gab=""http://zus.gov.pl/b2b/zus/channel/gabinetowe"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema-instance"">
	<soapenv:Header/>
	<soapenv:Body>
		<gab:pobierzOswiadczenie />
	</soapenv:Body>
</soapenv:Envelope>";
			XDocument result = SendMessage(message, "zus_channel_zla_Binder_pobierzOswiadczenie");
			return result;
		}

		public XDocument SendMessage(string msg, string action)
		{
			XDocument message = XDocument.Parse(msg);
			HttpWebRequest webRequest = CreateWebRequest(action);
			using (Stream stream = webRequest.GetRequestStream())
			{
				message.Save(stream);
			}

			IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			string soapResult;
			using (WebResponse WR = webRequest.EndGetResponse(asyncResult))
			{
				using (StreamReader SR = new StreamReader(WR.GetResponseStream()))
				{
					soapResult = SR.ReadToEnd();
				}
			}

			return XDocument.Parse(soapResult);
		}

		private HttpWebRequest CreateWebRequest(string action)
		{
			HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(zusWebServiceAddress);
			WR.Headers.Add("SOAPAction", action);
			WR.ContentType = "text/xml; charset=\"utf-8\"";
			WR.Accept = "text/xml";
			WR.Method = "POST";
			WR.Credentials = new NetworkCredential("ezla_ag", "ezla_ag");
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

	}
}
