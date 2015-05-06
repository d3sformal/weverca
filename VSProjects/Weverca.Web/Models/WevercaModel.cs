/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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

            [LocalizedDescription(typeof(Resources), "SimpleTestsDescription")]
            SimpleTests,

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
            else if (Input == InputType.SimpleTests)
            {
                PhpCode = Resources.SimpleTests;
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
            else if (PhpCode == Resources.SimpleTests)
            {
                Input = InputType.SimpleTests;
            }
            else
            {
                Input = InputType.Custom;
            }
        }

        #endregion
    }
}