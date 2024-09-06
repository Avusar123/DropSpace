﻿using System.ComponentModel.DataAnnotations;

namespace DropSpace.Models
{
    public class CreateSessionViewModel
    {
        [MaxLength(30, ErrorMessage = "Превышен лимит названия сессии!")]
        [Required(ErrorMessage = "Это поле обязательно!")]
        public string Name { get; set; }
    }
}
