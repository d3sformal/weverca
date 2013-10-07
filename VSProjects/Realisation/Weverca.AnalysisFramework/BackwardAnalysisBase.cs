using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework
{

    /// <summary>
    /// TODO: How to resolve steps (it cause many duplications with forward resolvers)
    /// </summary>
    public abstract class BackwardAnalysisBase
    {
        ProgramPointGraph _graph;

        public bool IsAnalyzed { get; private set; }

        public BackwardAnalysisBase(ProgramPointGraph graph)
        {
            _graph = graph;
        }

        public void Analyse()
        {
            
            IsAnalyzed = true;
        }

        private void checkAlreadyAnalyzed()
        {
        }
    }
}
