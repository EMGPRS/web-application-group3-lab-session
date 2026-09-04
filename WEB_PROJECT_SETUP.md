# Employee Directory Web Project Setup

This guide explains how the MVC Web project calls the Employee Directory API, how to add another Web project to this solution, and how to run the API and Web projects together in Visual Studio 2026.

## Projects

- `EmployeeDirectory.Api`: ASP.NET Core Web API that exposes employee endpoints.
- `EmployeeDirectory.Web`: ASP.NET Core MVC application that displays and changes employee data through the API.

The solution file is `EmployeeDirectory.Api.slnx`.

## Add a Web Project to the Solution

### Visual Studio 2026

1. Open `EmployeeDirectory.Api.slnx`.
2. In Solution Explorer, right-click the solution.
3. Select **Add** > **New Project**.
4. Choose **ASP.NET Core Web App (Model-View-Controller)**.
5. Set the project name, for example `EmployeeDirectory.Web`.
6. Select the same target framework as the API (`net10.0` in this solution).
7. Select **Create**.

## Find the API Base URL

Open `EmployeeDirectory.Api/Properties/launchSettings.json` and find `applicationUrl` in the launch profile you plan to run.

This project's API profiles are:

| Profile | API base URL |
| --- | --- |
| `http` | `http://localhost:5244/` |
| `https` | `https://localhost:7227/` |

When Visual Studio starts the API with the `https` profile, configure the Web project with:

```json
"EmployeeDirectoryApi": {
  "BaseUrl": "https://localhost:7227/"
}
```

The trailing slash is important because relative paths such as `api/employee` are appended to this base URL.

## Configure the Web Project

Add an API configuration section to `EmployeeDirectory.Web/appsettings.json`:

```json
{
  "EmployeeDirectoryApi": {
    "BaseUrl": "https://localhost:7227/"
  }
}
```

Register a named HTTP client in `EmployeeDirectory.Web/Program.cs`, after `AddControllersWithViews()` and before `builder.Build()`:

```csharp
builder.Services.AddHttpClient("EmployeeDirectoryApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["EmployeeDirectoryApi:BaseUrl"]!);
});
```

Use a different value in `appsettings.Development.json` when your local API runs on another URL. Development settings override `appsettings.json`.

## Add the Web Models and View Model

The Web project needs its own models because it receives JSON from the API. Keep their property names and types aligned with the API contract.

Create `EmployeeDirectory.Web/Models/Employee.cs`:

```csharp
namespace EmployeeDirectory.Web.Models
{
    public enum Gender
    {
        Male = 1,
        Female = 2,
    }

    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public Gender Gender { get; set; }
    }
}
```

Create `EmployeeDirectory.Web/Models/EmployeeViewModel.cs`:

```csharp
namespace EmployeeDirectory.Web.Models
{
    public class EmployeeViewModel
    {
        public List<Employee> Employees { get; set; }
        public Employee Employee { get; set; }
    }
}
```

`EmployeeViewModel` gives the Index view both the employee list and one employee instance for the add/edit form.

## Update the MVC Controller

Inject `IHttpClientFactory` into `HomeController` and create the named client:

```csharp
private readonly HttpClient _httpClient;

public HomeController(IHttpClientFactory httpClientFactory)
{
    _httpClient = httpClientFactory.CreateClient("EmployeeDirectoryApi");
}
```

Add `using System.Net.Http.Json;` so the JSON HTTP extension methods are available.

The API route is `api/employee`, based on the API controller route `[Route("api/[controller]")]`.

```csharp
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
```

## Update the Index View

At the top of `Views/Home/Index.cshtml`, specify the view model:

```cshtml
@model EmployeeViewModel
```

Use `Model.Employees` to create the table rows. Bind the form fields to `Employee`, such as `asp-for="Employee.FirstName"`. The form should post to `Save`; edit links should pass the employee ID with `asp-route-id`; and delete forms should post to `Delete` with a hidden `id` field.

For the `Gender` enum, use its numeric values in the select options:

```cshtml
<option value="1">Male</option>
<option value="2">Female</option>
```

## Run API and MVC Web Projects Together in Visual Studio 2026

1. Open the solution in Visual Studio 2026.
2. Right-click the solution in Solution Explorer and select **Configure Startup Projects**.
3. Select **Multiple startup projects**.
4. Set both projects to **Start**:
   - `EmployeeDirectory.Api`
   - `EmployeeDirectory.Web`
5. Make sure the API uses the `https` profile when the Web configuration points to `https://localhost:7227/`.
6. Select **OK**, then press `F5` or choose **Start**.

The API must be running before the Web app sends requests to `api/employee`.

## Verify

After changing models, configuration, controller, or views, select **Build** > **Build Solution** in Visual Studio.

With both applications running, open the MVC Web application, add an employee, edit it, and delete it. Each operation should be reflected by the API response and the refreshed employee list.