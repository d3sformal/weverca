using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Weverca.Web.Properties;

namespace Weverca.Web.Models
{
    public class WevercaModel
    {
        #region Enums
        
        public enum InputType { Input1, Input2, Custom}
        
        #endregion

        #region Fields

        string phpCode;

        #endregion

        #region Properties

        [Display(ResourceType = typeof(Weverca.Web.Properties.Resources), Name = "PhpCode")]
        public string PhpCode
        {
            get
            {
                phpCode = GetPhpCode(Input);
                return phpCode;
            }
            set
            {
                phpCode = value;
            }
        }

        [Display(ResourceType = typeof(Weverca.Web.Properties.Resources), Name = "InputType")]
        public InputType Input { get; set; }

        public bool Verify { get; set; }

        #endregion
        
        #region Private Methods

        string GetPhpCode(InputType input)
        {
            if (input == InputType.Input1)
            {
                return Resources.Input1;
            }
            else if (input == InputType.Input2)
            {
                return Resources.Input2;
            }

            return phpCode;
        }

        #endregion
    }
}