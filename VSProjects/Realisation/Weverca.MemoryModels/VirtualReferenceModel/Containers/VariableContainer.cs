/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Memory;

using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Containers
{
    /// <summary>
    /// Storage holding variable values
    /// </summary>
    class VariableContainer
    {
        private Dictionary<string, VariableInfo> _oldVariables = new Dictionary<string, VariableInfo>();

        private Dictionary<string, VariableInfo> _variables = new Dictionary<string, VariableInfo>();

        private readonly Snapshot _owner;

        private readonly VariableContainer _globals;

        private bool CanFetchFromGlobals { get { return _globals != null; } }

        internal readonly VariableKind DefaultVariableKind;

        internal IEnumerable<VariableInfo> VariableInfos { get { return _variables.Values; } }

        internal IEnumerable<string> VariableIdentifiers { get { return _variables.Keys; } }

        internal bool DifferInCount { get { return _variables.Count != _oldVariables.Count; } }

        internal IEnumerable<VariableName> VariableNames
        {
            get
            {
                foreach (var key in _variables.Keys)
                {
                    yield return new VariableName(key);
                }
            }
        }

        internal VariableContainer(VariableKind defaultVariableKind, Snapshot owner, VariableContainer globals = null)
        {
            DefaultVariableKind = defaultVariableKind;
            _owner = owner;
            _globals = globals;
        }

        #region Container API for content manipulation

        /// <summary>
        /// Determine that change according to oldVariables is present
        /// </summary>
        /// <returns>True if change is detected, false otherwise</returns>
        internal bool CheckChange()
        {
            foreach (var oldVar in _oldVariables)
            {
                REPORT(Statistic.SimpleHashSearches);
                VariableInfo currVar;
                if (!_variables.TryGetValue(oldVar.Key, out currVar))
                {
                    //differ in some variable presence
                    return true;
                }

                if (!currVar.Equals(oldVar.Value))
                {
                    //differ in variable definition
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Flip old/current buffers
        /// </summary>
        internal void FlipBuffers()
        {
            var swap = _oldVariables;
            _oldVariables = _variables;
            _variables = swap;

            //Prepare clean buffer for new writing
            _variables.Clear();
        }

        /// <summary>
        /// Clear data stored in current buffer
        /// </summary>
        internal void ClearCurrent()
        {
            _variables.Clear();
        }

        /// <summary>
        /// Fetch variable according to info
        /// </summary>
        /// <param name="fetchedVariable"></param>
        internal void Fetch(VariableInfo fetchedVariable)
        {
            REPORT(Statistic.SimpleHashAssigns);
            _variables[fetchedVariable.Name.Value] = fetchedVariable;
        }

        /// <summary>
        /// Get VariableInfo of given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        internal bool TryGetValue(VariableName name, out VariableInfo result)
        {
            REPORT(Statistic.SimpleHashSearches);
            return _variables.TryGetValue(name.Value, out result);
        }

        /// <summary>
        /// Set variable info of given name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="result"></param>
        internal void SetValue(VariableName name, VariableInfo result)
        {
            REPORT(Statistic.SimpleHashAssigns);
            _variables[name.Value] = result;
        }

        /// <summary>
        /// Determine that container contains variable info of given name
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        internal bool ContainsKey(VariableName variableName)
        {
            REPORT(Statistic.SimpleHashSearches);
            return _variables.ContainsKey(variableName.Value);
        }

        #endregion

        #region Extension handling (TODO needs refactoring)

        internal void ExtendBy(VariableContainer variableContainer, bool directExtend)
        {
            foreach (var varPair in variableContainer._variables)
            {
                VariableInfo oldVar;
                REPORT(Statistic.SimpleHashSearches);
                if (!_variables.TryGetValue(varPair.Key, out oldVar))
                {
                    //copy variable info, so we can process changes on it
                    REPORT(Statistic.SimpleHashAssigns);
                    if (CanFetchFromGlobals && varPair.Value.IsGlobal)
                    {
                        //fetch from globals
                        _variables[varPair.Key] = _globals._variables[varPair.Key];
                    }
                    else
                    {
                        var clone = varPair.Value.Clone();
                        if (!directExtend)
                        {
                            //undefined branch
                            //TODO what about callback references ?
                            var reference = new VirtualReference(clone, _owner.CurrentContextStamp);

                            if (!clone.References.Contains(reference))
                                clone.References.Add(reference);
                        }

                        _variables[varPair.Key] = clone;
                    }
                }
                else
                {
                    //merge variable references
                    foreach (var reference in varPair.Value.References)
                    {
                        if (!oldVar.References.Contains(reference))
                        {
                            oldVar.References.Add(reference);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private utilities

        private void REPORT(Statistic statistic)
        {
            _owner.ReportStatistic(statistic);
        }

        #endregion

        #region String representation building

        public override string ToString()
        {
            return Representation;
        }

        public string Representation
        {
            get
            {
                return GetRepresentation();
            }
        }

        private string GetRepresentation()
        {
            var result = new StringBuilder();

            foreach (var variableInfo in VariableInfos)
            {
                if (variableInfo.Kind != DefaultVariableKind)
                {
                    result.AppendFormat("{0}: #Fetched from {1}", variableInfo, variableInfo.Kind);
                    result.AppendLine();
                }
                else
                {
                    result.AppendFormat("{0}: {{", variableInfo);

                    var entry = _owner.ReadValue(new VariableKey(variableInfo.Kind, variableInfo.Name, _owner.CurrentContextStamp), false);
                    foreach (var value in entry.PossibleValues)
                    {
                        result.AppendFormat("'{0}', ", value);
                    }

                    result.Length -= 2;
                    result.AppendLine("}");
                }
            }

            return result.ToString();
        }
        #endregion

    }

}