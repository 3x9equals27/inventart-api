namespace Inventart.Models.ControllerInputs
{
    public class AuthRegister
    {
        ///<example>something@gmail.com</example>
        public string Email { get; set; }
        ///<example>1234</example>
        public string Password { get; set; }
        ///<example>1234</example>
        public string PasswordRepeat { get; set; }
        ///<example>John</example>
        public string FirstName { get; set; }
        ///<example>Doe</example>
        public string LastName { get; set; }
    }
}
