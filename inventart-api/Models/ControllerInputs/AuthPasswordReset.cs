using System;

namespace Inventart.Models.ControllerInputs
{
    public class AuthPasswordReset
    {
        public Guid PasswordResetGuid { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}
