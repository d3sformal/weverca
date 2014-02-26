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
            [LocalizedDescription(typeof(Resources), "SimpleExample")]
            SimpleExample,

            [LocalizedDescription(typeof(Resources), "EndlessLoop")]
            EndlessLoop,

            [LocalizedDescription(typeof(Resources), "FractalDescription")]
            Fractal,

            [LocalizedDescription(typeof(Resources), "MetalcupStatisDescription")]
            MetalcupStatis,

            [LocalizedDescription(typeof(Resources), "RandomizeDescription")]
            Randomize,

            [LocalizedDescription(typeof(Resources), "SimpleObjectDescription")]
            SimpleObject,

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

        public AnalysisModel AnalysisModel { get; set; }

        #endregion

        #region Constructor

        public WevercaModel()
        {
            Input = InputType.SimpleExample;
            AssignInput();
            AnalysisModel = new AnalysisModel();
        }

        #endregion

        #region Methods

        public void AssignInput()
        {
            if (Input == InputType.SimpleExample)
            {
                PhpCode = Resources.Input1;
            }
            else if (Input == InputType.EndlessLoop)
            {
                PhpCode = Resources.Input2;
            }
            else if (Input == InputType.Fractal)
            {
                PhpCode = Resources.Fractal;
            }
            else if (Input == InputType.MetalcupStatis)
            {
                PhpCode = Resources.MetalcupStatis;
            }
            else if (Input == InputType.Randomize)
            {
                PhpCode = Resources.Randomize;
            }
            else if (Input == InputType.SimpleObject)
            {
                PhpCode = Resources.SimpleObject;
            }
        }

        public void AssignInputType()
        {
            if (PhpCode == Resources.Input1)
            {
                Input = InputType.SimpleExample;
            }
            else if (PhpCode == Resources.Input2)
            {
                Input = InputType.EndlessLoop;
            }
            else if (PhpCode == Resources.Fractal)
            {
                Input = InputType.Fractal;
            }
            else if (PhpCode == Resources.MetalcupStatis)
            {
                Input = InputType.MetalcupStatis;
            }
            else if (PhpCode == Resources.Randomize)
            {
                Input = InputType.Randomize;
            }
            else if (PhpCode == Resources.SimpleObject)
            {
                Input = InputType.SimpleObject;
            }
            else
            {
                Input = InputType.Custom;
            }
        }

        #endregion
    }
}