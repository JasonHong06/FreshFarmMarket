using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Diagnostics;

namespace FreshFarmMarket.Pages.Errors
{
    public class ErrorModel : PageModel
    {
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet(int? statusCode)
        {
            StatusCode = statusCode ?? 500;
            ErrorMessage = StatusCode switch
            {
                400 => "Bad Request - The server could not understand the request.",
                401 => "Unauthorized - You must be logged in to access this page.",
                403 => "Forbidden - You do not have permission to access this page.",
                404 => "Page Not Found - The requested page does not exist.",
                500 => "Internal Server Error - Something went wrong on our end.",
                _ => "An unexpected error occurred."
            };
        }
    }
}
