using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DropSpace.Models.Data
{
    public class User
    {
        [BindNever]
        public Guid Guid { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }


        [BindNever]
        [ValidateNever]
        [PasswordPropertyText]
        public string PasswordHash { get; set; }

        [Required]
        [FromForm]
        [PasswordPropertyText]
        [MaxLength(15)]
        [NotMapped]
        public string Password { get; set; }
    }
}
