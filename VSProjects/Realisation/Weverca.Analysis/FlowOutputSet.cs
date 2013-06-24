using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis
{
    /// <summary>
    /// Set of FlowInfo used as output from statement analysis.   
    /// </summary>
    public class FlowOutputSet : FlowInputSet
    {
        public ISnapshotReadWrite Output { get { return _snapshot; } }

        public bool HasChanges { get; private set; }
        

        internal FlowOutputSet(AbstractSnapshot snapshot) :
            base(snapshot)
        {
            //because new snapshot has been initialized
            HasChanges = true; 
        }

        internal void CommitTransaction()
        {
            _snapshot.CommitTransaction();
            HasChanges = _snapshot.HasChanged;
        }

        internal void StartTransaction()
        {
            _snapshot.StartTransaction();
        }

        internal void ResetChanges()
        {
            HasChanges = false;
        }
    }
}
