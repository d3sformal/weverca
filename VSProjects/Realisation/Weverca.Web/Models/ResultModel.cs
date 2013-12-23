using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Weverca.Web.Models
{
    public class ResultModel
    {
        public string PhpCode { get; private set; }

        public ResultModel(string phpCode)
        {
            PhpCode = phpCode;
        }
    }
}