using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ANIMAUX.Models
{
    public class RegistrationPublication
    {
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }


        [Required(ErrorMessage ="Введите ссылку")]
        [Url]
        [Display(Name = "Фото")]
        public string Foto { get; set; }

        [Required(ErrorMessage = "Введите название города")]
        [Display(Name = "Город")]
        public string Sity { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Выберите статус животного")]
        [Display(Name = "Статус")]
        public string Status { get; set; }



    }
}