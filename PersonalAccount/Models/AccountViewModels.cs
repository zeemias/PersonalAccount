using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PersonalAccount.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Код")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Запомнить этот браузер?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Поле обязательно для заполнения!")]
        [Display(Name = "Логин")]
        [EmailAddress(ErrorMessage = "Неправильный формат логина!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения!")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail организации:")]
        public string Email { get; set; }

        [Display(Name = "Вид организации:")]
        public string CompanyType { get; set; }

        [Required]
        [Display(Name = "Организационно-правовая форма:")]
        public string OPF { get; set; }

        [Display(Name = "Название компании:")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Полное нименование оценочной компании:")]
        public string FullCompanyName { get; set; }

        [Required]
        [Display(Name = "Город:")]
        public string City { get; set; }

        [Required]
        [Display(Name = "ОГРН/ОГРНИП:")]
        public string OGRN { get; set; }

        [Required]
        [Display(Name = "ФИО контактного лица:")]
        public string ContactFIO { get; set; }

        [Required]
        [Display(Name = "Основной телефон:")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Доп. телефон 1:")]
        public string PhoneNumberOne { get; set; }

        [Display(Name = "Доп. телефон 2:")]
        public string PhoneNumberTwo { get; set; }

        [EmailAddress]
        [Display(Name = "E-mail сотрудника:")]
        public string EmailEmployee { get; set; }

        [Display(Name = "Web-сайт:")]
        public string WebSite { get; set; }

        [Display(Name = "ИНН:")]
        public string INN { get; set; }

        [Display(Name = "КПП:")]
        public string KPP { get; set; }

        [Display(Name = "Юридический адрес:")]
        public string LawAddress { get; set; }

        [Display(Name = "ФИО директора:")]
        public string DirectorFIO { get; set; }

        [Display(Name = "Должность:")]
        public string DirectorPost { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}
