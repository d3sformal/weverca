using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


using PHP.Library;
using PHP.Core;

namespace Weverca.Function.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Dictionary<string, List<MethodInfo>> phpMethods=new  Dictionary<string, List<MethodInfo>>();
            Console.WriteLine("works");

            string input = "1123581321345589aaabbaaab";
            Console.WriteLine(PhpStrings.Reverse(input));
            //no PhpDocumentation PhpFile PhpDirectory PhpGlob PhpPath PhpFunctions Highlighting Mailer Misc Sockets PhpObjects Output PhpJson PhpSession Shell PhpFilters PhpConstants PhpVariables Web
            Type[] types = { typeof(PhpArrays), typeof(PhpStrings), typeof(PhpBitConverter), typeof(Errors), typeof(PhpFiltering), typeof(PhpHash), typeof(CharType), typeof(PhpMath), typeof(PerlRegExp), typeof(PosixRegExp), typeof(UUEncoder) };


            foreach(var type in types)
            {
                foreach (var a in type.GetMethods())
                {
                    foreach (var b in a.CustomAttributes)
                    {
                        if (b.AttributeType.Name=="ImplementsFunctionAttribute")
                        {
                            string key = (string)b.ConstructorArguments[0].Value;
                            if (!phpMethods.ContainsKey(key))
                            {
                                phpMethods[key] = new List<MethodInfo>();
                            }
                           
                                 phpMethods[key].Add(a);
                        
                        }
                    }
                }
            }
            foreach (var a in phpMethods.Keys)
            {
                Console.WriteLine(a + " " + phpMethods[a].Count);  
            }


            
        }
    }
}
