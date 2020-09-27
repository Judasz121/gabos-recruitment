using Gabos_recruitmentTest_web_app.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Gabos_recruitmentTest_web_app.Controllers
{
	public class CSharpTestController : Microsoft.AspNetCore.Mvc.Controller
	{
		private readonly ZusWebServiceController _zusController;
		private readonly GusWebServiceController _gusController;
		public CSharpTestController(ZusWebServiceController zusController, GusWebServiceController gusController)
		{
			_zusController = zusController;
			_gusController = gusController;
		}
		[HttpGet]
		public IActionResult TaskOne()
		{
			var model = new CSharpTaskOneViewModel();
			XDocument responseXml = _zusController.pobierzOswiadczenie();

			string responseString = _zusController.ConvertXmlToString(responseXml);
			model.zusResponse = responseString;
			return View(model);
		}

		[HttpPost]
		public IActionResult TaskOne(Models.CSharpTaskOneViewModel model)
		{
			DateTime dateIssued = DateTime.Parse(model.dateIssued);
			DateTime inabilityToWorkFrom = DateTime.Parse(model.inabilityToWorkFromDate);
			DateTime inabilityToWorkTo = DateTime.Parse(model.inabilityToWorkToDate);
			DateTime inHospitalFrom = DateTime.Parse(model.inHospitalFromDate);
			DateTime inHospitalTo = DateTime.Parse(model.inHospitalToDate);
			model.ZLAdocs = CalcZLAdates(dateIssued, inabilityToWorkFrom, inabilityToWorkTo, inHospitalFrom, inHospitalTo);

			XDocument responseXml = _zusController.pobierzOswiadczenie();
			string responseString = _zusController.ConvertXmlToString(responseXml);
			model.zusResponse = responseString;


			return View(model);
		}
		public ZLAdoc[] CalcZLAdates(DateTime dateIssued, DateTime inabilityToWorkFrom, DateTime inabilityToWorkTo, DateTime inHospitalFrom, DateTime inHospitalTo)
		{
			ZLAdoc currentCertificate = new ZLAdoc();
			ZLAdoc retrospectiveCertificate = new ZLAdoc();

			if(DateTime.Compare(dateIssued, inHospitalFrom) > 0)
			{// already in hospital
				retrospectiveCertificate.inabilityToWorkFrom = inabilityToWorkFrom;
				retrospectiveCertificate.inabilityToWorkTo = inHospitalFrom.Subtract(new TimeSpan(4, 0, 0, 0));
				retrospectiveCertificate.inHospitalFrom = null;
				retrospectiveCertificate.inHospitalTo = null;
				retrospectiveCertificate.typeOfDoc = "wsteczne";

				currentCertificate.inabilityToWorkFrom = inHospitalFrom.Subtract(new TimeSpan(3, 0, 0, 0));
				currentCertificate.inabilityToWorkTo = inabilityToWorkTo;
				currentCertificate.inHospitalFrom = inHospitalFrom;
				currentCertificate.inHospitalTo = inHospitalTo;
				currentCertificate.typeOfDoc = "bieżące";
			}
			else
			{// before hospital
				retrospectiveCertificate.inabilityToWorkFrom = inabilityToWorkFrom;
				retrospectiveCertificate.inabilityToWorkTo = dateIssued.Subtract(new TimeSpan(4, 0, 0, 0));
				retrospectiveCertificate.inHospitalFrom = null;
				retrospectiveCertificate.inHospitalTo = null;
				retrospectiveCertificate.typeOfDoc = "wsteczne";

				currentCertificate.inabilityToWorkFrom = dateIssued.Subtract(new TimeSpan(3, 0, 0, 0));
				currentCertificate.inabilityToWorkTo = inabilityToWorkTo;
				currentCertificate.inHospitalFrom = inHospitalFrom;
				currentCertificate.inHospitalTo = inHospitalTo;
				currentCertificate.typeOfDoc = "bieżące";
			}

			return new ZLAdoc[2] { retrospectiveCertificate, currentCertificate };
		}

		[HttpGet]
		public IActionResult TaskTwo()
		{
			var model = new CSharpTaskTwoViewModel();
			return View(model);
		}
		[HttpPost]
		public IActionResult TaskTwo(CSharpTaskTwoViewModel model)//  5261040828, 6452521870, 9521975074
		{
			string searchParameters = model.searchParameters;
			string[] NIPs;
			if (searchParameters.Contains(','))
				NIPs = searchParameters.Replace(" ", "").Split(new char[1] { ',' });
			else
				NIPs = new string[] { searchParameters.Trim(' ') };

			_gusController.Zaloguj();
			model.gusResponse = _gusController.DaneSzukajPodmioty(NIPs);
			return View(model);
		}

	}
}
