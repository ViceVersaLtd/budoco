using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace budoco.Pages.Admin
{
    public class DatabaseTestModel : PageModel
    {
        public string TestResults { get; set; } = "";

        public void OnGet()
        {
            // Check if user is admin
            if (!bd_util.is_user_admin(HttpContext))
            {
                Response.Redirect("/");
                return;
            }
        }

        public void OnPost()
        {
            // Check if user is admin
            if (!bd_util.is_user_admin(HttpContext))
            {
                Response.Redirect("/");
                return;
            }

            try
            {
                TestResults = DatabaseCompatibilityTest.GetTestResults();
            }
            catch (System.Exception ex)
            {
                TestResults = $"Error running tests: {ex.Message}\n\nStack trace:\n{ex.StackTrace}";
            }
        }
    }
}