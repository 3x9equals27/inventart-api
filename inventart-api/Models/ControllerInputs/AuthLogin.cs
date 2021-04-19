namespace Inventart.Models.ControllerInputs
{
    public class AuthLogin
    {
        ///<example>guest@inventart.com</example>
        public string Email { get; set; }
        ///<example>guest</example>
        public string Password { get; set; }
    }
}
