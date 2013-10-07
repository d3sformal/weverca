using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Set of FlowInfo used as output from statement analysis.
    /// NOTE: Provides API for call dispatching, type resolving and include dispatching.
    /// </summary>
    /// <typeparam name="FlowInfo">Type of object which hold information collected during statement analysis.</typeparam>
    public class FlowOutputSet<FlowInfo> : FlowInputSet<FlowInfo>
    {
    
        public FlowOutputSet()
        {
        }

        /// <summary>
        /// Private copy constructor.
        /// </summary>
        /// <param name="output">Copied set</param>
        private FlowOutputSet(FlowOutputSet<FlowInfo> output)
        {
            collectedInfo = new Dictionary<object, FlowInfo>(output.collectedInfo);
            functions = new Dictionary<QualifiedName, FunctionDecl>(output.functions);
        }


        /// <summary>
        /// Set info for given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        public void SetInfo(object key, FlowInfo info)
        {
            collectedInfo[key] = info;
        }

        internal FlowOutputSet<FlowInfo> Copy()
        {
            return new FlowOutputSet<FlowInfo>(this);
        }

        public void FillFrom(FlowInputSet<FlowInfo> inSet)
        {
            foreach (var key in inSet.CollectedKeys)
            {
                FlowInfo info;
                System.Diagnostics.Debug.Assert(inSet.TryGetInfo(key, out info));
                SetInfo(key, info);
            }

            foreach (var fn in inSet.CollectedFunctions)
            {
                SetDeclaration(fn);
            }
        }

        public void SetDeclaration(FunctionDecl x)
        {
            functions[x.Function.QualifiedName] = x;
        }
    }
}
