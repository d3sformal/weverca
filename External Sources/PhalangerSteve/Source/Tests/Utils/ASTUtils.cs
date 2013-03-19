using PHP.Core;
using PHP.Core.AST;
using PhpRefactoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHP.Tests.Utils
{
    public static class ASTUtils
    {
        public static GlobalCode GetAST(this string code)
        {
            ICodeComments codeComments;
            var result = PhpRefactoring.Utils.CustomCompilationUnit.ParseCode("./file.php", code, out codeComments);
            Debug.Assert(result is GlobalCode);
            return (GlobalCode)result;
        }
    }
}
