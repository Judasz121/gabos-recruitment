using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Gabos_recruitmentTest_web_app.Controllers
{
	public class SqlTestController : Controller
	{
		public IActionResult TaskOne()
		{
			return View();
		}
		public IActionResult TaskTwo()
		{
			return View();
		}
	}
}
