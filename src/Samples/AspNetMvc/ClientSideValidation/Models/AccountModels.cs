using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ClientSideValidation.Models
{

    #region Models

    public class LogOnModel
    {
        [Required]
       // [Range(10, 200)]
        [StringLength(10)]
        public string UserName { get; set; }
        [Required]
        [RegularExpression(@"^\d+$")]
        public string Password { get; set; }
    }
    #endregion
}
