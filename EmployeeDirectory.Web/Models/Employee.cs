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