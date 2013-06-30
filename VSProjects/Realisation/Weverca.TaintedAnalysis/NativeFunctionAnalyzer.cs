using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;
using PHP.Core;

namespace Weverca.TaintedAnalysis
{
    class NativeFunctionAnalyzer
    {
        bool existNativeFunction(QualifiedName name)
        {

            return false;
        }
        QualifiedName[] getNativeFunctions()
        {
            return null;
        }
        NativeAnalyzerMethod getNativeAnalyzer(QualifiedName name)
        {
            return null;
        }

    }    
}
