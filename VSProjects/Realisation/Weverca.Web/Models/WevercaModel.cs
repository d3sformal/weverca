using System.ComponentModel.DataAnnotations;

using Common.WebDefinitions.Localization;
using Weverca.Web.Properties;

namespace Weverca.Web.Models
{
    public class WevercaModel
    {
        #region Enums

        public enum InputType
        {
            [LocalizedDescription(typeof(Resources), "Input1Description")]
            Input1,

            [LocalizedDescription(typeof(Resources), "Input2Description")]
            Input2,

            [LocalizedDescription(typeof(Resources), "InputCustomDescription")]
            Custom
        }
        
        #endregion

        #region Properties

        public bool ChangeInput { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "PhpCode")]
        public string PhpCode { get; set; }

        [Display(ResourceType = typeof(Resources), Name = "InputType")]
        public InputType Input { get; set; }

        #endregion

        #region Constructor

        public WevercaModel()
        {
            Input = InputType.Input1;
            AssignInput();
        }

        #endregion

        #region Methods

        public void AssignInput()
        {
            if (Input == InputType.Input1)
            {
                PhpCode = Resources.Input1;
            }
            else if (Input == InputType.Input2)
            {
                PhpCode = Resources.Input2;
            }
        }

        public void AssignInputType()
        {
            if (PhpCode == Resources.Input1)
            {
                Input = InputType.Input1;
            }
            else if (PhpCode == Resources.Input2)
            {
                Input = InputType.Input2;
            }
            else
            {
                Input = InputType.Custom;
            }
        }

        #endregion
    }
}