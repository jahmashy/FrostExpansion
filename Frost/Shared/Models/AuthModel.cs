using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace Frost.Shared.Models
{
    internal class AuthModel
    {
        
    }
    public class LoginResult
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string Role { get; set; }
        public int telNumber { get; set; }
        public string jwt { get; set; }
        public DateTime jwtExpDate { get; set; }
        public string jwtRefresh { get; set; }
    }
    public class LoginModel
    {
       
        [Required(ErrorMessageResourceName = "EmailRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [EmailAddress(ErrorMessageResourceName = "EmailValidError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string email { get; set; }
        [Required(ErrorMessageResourceName = "PasswordRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [DataType(DataType.Password)]
        public string password { get; set; }
    }
    public class RegistrationModel : LoginModel
    {
        [Required(ErrorMessageResourceName = "ConfirmPasswordRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [DataType(DataType.Password)]
        [Compare("password", ErrorMessageResourceName = "PasswordsDoNotMatchError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string confirmPassword { get; set; }

        [MaxLength(20, ErrorMessageResourceName = "NameMaxLengthError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [Required(ErrorMessageResourceName = "NameRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string name { get; set; }

        [Required(ErrorMessageResourceName = "TelNumberRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [DataType(DataType.PhoneNumber)]
        [Range(111111111,999999999, ErrorMessageResourceName = "TelNumberIsInvalidError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public int? telNumber { get; set; }
    }
    public class ChangeLoginDetailsModel
    {
        [Required(ErrorMessageResourceName = "EmailRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [EmailAddress(ErrorMessageResourceName = "EmailValidError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string email { get; set; }
        [DataType(DataType.Password)]
        public string? password { get; set; }
        [Required(ErrorMessageResourceName = "TelNumberRequiredError", ErrorMessageResourceType = typeof(Resources.Errors))]
        [DataType(DataType.PhoneNumber)]
        [Range(111111111, 999999999, ErrorMessageResourceName = "TelNumberIsInvalidError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public int? telNumber { get; set; }
        [DataType(DataType.Password)]
        public string? newPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("newPassword", ErrorMessageResourceName = "PasswordsDoNotMatchError", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string? confirmNewPassword { get; set; }
    }
}
