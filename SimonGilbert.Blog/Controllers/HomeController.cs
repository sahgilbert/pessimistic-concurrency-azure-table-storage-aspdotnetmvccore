using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimonGilbert.Blog.Models;
using SimonGilbert.Blog.Services;

namespace SimonGilbert.Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOrderService _orderService;
        private const string AccountId = "Simon Gilbert";
        private const string OrderId = "123456789";

        public HomeController(IOrderService orderService)
        {
            this._orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _orderService.Create(AccountId, OrderId);

            var order = await _orderService.Get(AccountId, OrderId);

            return View(order);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
