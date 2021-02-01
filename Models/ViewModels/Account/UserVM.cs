using StoreTrainee.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StoreTrainee.Models.ViewModels.Account
{
    public class UserVM
    {
        public UserVM()
        {
        }

        public UserVM(UserDTO row)
        {
            Id = row.Id;
            FirstName =row.FirstName ;
            LastName = row.LastName;
            EmailAdress = row.EmailAdress;
            Username = row.Username;
            Password = row.Password;
            Phone = row.Phone;
            Adress = row.Adress;
        }

        public int Id { get; set; }
        [Required]
        [DisplayName("Имя")]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }
        [Required]
        [DisplayName("Электронная почта")]
        [DataType(DataType.EmailAddress)]
        public string EmailAdress { get; set; }
        [Required]
        [DisplayName("Логин")]
        public string Username { get; set; }
        [Required]
        [DisplayName("Пароль")]
        public string Password { get; set; }
        [Required]
        [DisplayName("Подтверждение пароля")]
        public string ConfirmPassword { get; set; }

        //////////////
        [Required]
        [DisplayName("Номер телефона")]
        public string Phone { get; set; }
        [Required]
        [DisplayName("Адрес доставки")]
        public string Adress { get; set; }


    }
}