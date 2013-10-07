using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// Set of FlowInfo used as input for statement analysis.
    /// </summary>
    /// <typeparam name="FlowInfo">Type of object which hold information collected during statement analysis.</typeparam>
    public class FlowInputSet<FlowInfo>
    {
        /// <summary>
        /// Info available in set.
        /// </summary>
        public IEnumerable<FlowInfo> CollectedInfo { get { return collectedInfo.Values; } }
        /// <summary>
        /// Keys for every available info.
        /// </summary>
        public IEnumerable<object> CollectedKeys { get { return collectedInfo.Keys; } }

        public IEnumerable<FunctionDecl> CollectedFunctions {get{return functions.Values;}}

        /// <summary>
        /// Info storage.
        /// </summary>
        protected Dictionary<object,FlowInfo> collectedInfo=new Dictionary<object,FlowInfo>();

        protected Dictionary<QualifiedName,FunctionDecl> functions=new Dictionary<QualifiedName,FunctionDecl>();

        /// <summary>
        /// Try to get info according to given key. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="info"></param>
        /// <returns>True if info is present, false otherwise.</returns>
        public bool TryGetInfo(object key, out FlowInfo info)
        {
            return collectedInfo.TryGetValue(key, out info);
        }

        public bool TryGetFunction(QualifiedName name, out FunctionDecl function)
        {
            return functions.TryGetValue(name, out function);
        }


        public override bool Equals(object obj)
        {
            var o = obj as FlowInputSet<FlowInfo>;
            if (o == null)
                return false;

            var sameCount=collectedInfo.Count == o.collectedInfo.Count;
            var sameEls = !collectedInfo.Except(o.collectedInfo).Any();
            return sameCount && sameEls;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            foreach (var val in collectedInfo.Values)
            {
                b.AppendLine(val.ToString());
            }
            return b.ToString();
        }
    }
}
