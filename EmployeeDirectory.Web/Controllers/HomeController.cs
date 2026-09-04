using EmployeeDirectory.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Json;

namespace EmployeeDirectory.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EmployeeDirectoryApi");
        }

        public async Task<IActionResult> Index(int? id)
        {
            var employees = await _httpClient.GetFromJsonAsync<List<Employee>>("api/employee") ?? [];
            var viewModel = new EmployeeViewModel
            {
                Employees = employees,
                Employee = id.HasValue
                    ? employees.FirstOrDefault(employee => employee.Id == id.Value) ?? new Employee()
                    : new Employee()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            if (employee.Id == 0)
            {
                await _httpClient.PostAsJsonAsync("api/employee", employee);
            }
            else
            {
                await _httpClient.PutAsJsonAsync($"api/employee/{employee.Id}", employee);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"api/employee/{id}");
            return RedirectToAction(nameof(Index));
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
