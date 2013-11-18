using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.VirtualReferenceModel.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.Containers
{
    class DataContainer
    {
        private Dictionary<VirtualReference, MemoryEntry> _oldData = new Dictionary<VirtualReference, MemoryEntry>();

        private Dictionary<VirtualReference, MemoryEntry> _data = new Dictionary<VirtualReference, MemoryEntry>();

        private readonly Snapshot _owner;

        internal bool DifferInCount { get { return _oldData.Count != _data.Count; } }

        internal DataContainer(Snapshot owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Determine that change according to oldVariables is present
        /// </summary>
        /// <returns>True if change is detected, false otherwise</returns>
        internal bool CheckChange()
        {
            foreach (var oldData in _oldData)
            {
                MemoryEntry currEntry;
                REPORT(Statistic.SimpleHashSearches);
                if (!_data.TryGetValue(oldData.Key, out currEntry))
                {
                    //differ in presence of some reference
                    return true;
                }

                REPORT(Statistic.MemoryEntryComparisons);
                if (!currEntry.Equals(oldData.Value))
                {
                    //differ in stored data
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
            var swap = _oldData;
            _oldData = _data;
            _data = swap;

            //Prepare clean buffer for new writing
            _data.Clear();
        }

        /// <summary>
        /// Clear data stored in current buffer
        /// </summary>
        internal void ClearCurrent()
        {
            _data.Clear();
        }

        /// <summary>
        /// Get memory entry stored at given reference
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        internal MemoryEntry GetEntry(VirtualReference reference)
        {
            MemoryEntry entry;
            REPORT(Statistic.SimpleHashSearches);
            if (_data.TryGetValue(reference, out entry))
            {
                return entry;
            }

            //reading of uninitialized reference (may happen when aliasing cross contexts)
            REPORT(Statistic.MemoryEntryCreation);
            return new MemoryEntry(_owner.UndefinedValue);
        }

        /// <summary>
        /// Set memory entry at given reference
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="entry"></param>
        internal void SetEntry(VirtualReference reference, MemoryEntry entry)
        {
            REPORT(Statistic.SimpleHashAssigns);
            _data[reference] = entry;
        }

        internal void WidenWith(MemoryAssistantBase assistant)
        {
            foreach (var oldData in _oldData)
            {
                MemoryEntry widenedEntry = null;

                MemoryEntry currEntry;
                REPORT(Statistic.SimpleHashSearches);
                if (_data.TryGetValue(oldData.Key, out currEntry))
                {
                    REPORT(Statistic.MemoryEntryComparisons);
                    if (!currEntry.Equals(oldData.Value))
                    {
                        //differ in stored data
                        widenedEntry = assistant.Widen(oldData.Value, currEntry);
                    }
                }
                else
                {
                    //differ in presence of some reference
                    REPORT(Statistic.MemoryEntryCreation);
                    widenedEntry = assistant.Widen(new MemoryEntry(), currEntry);
                }

                if (widenedEntry == null)
                    //there is no widening
                    continue;

                //apply widening
                _data[oldData.Key] = widenedEntry;
            }
        }

        #region Extension handling (TODO needs refactoring)

        internal void ExtendBy(DataContainer dataContainer, bool directExtend)
        {
            foreach (var dataPair in dataContainer._data)
            {
                MemoryEntry oldEntry;

                REPORT(Statistic.SimpleHashSearches);
                if (_data.TryGetValue(dataPair.Key, out oldEntry))
                {
                    REPORT(Statistic.MemoryEntryComparisons);
                    if (!dataPair.Value.Equals(oldEntry))
                    {
                        if (directExtend)
                        {
                            REPORT(Statistic.SimpleHashAssigns);
                            _data[dataPair.Key] = dataPair.Value;
                        }
                        else
                        {
                            //merge two memory entries
                            REPORT(Statistic.MemoryEntryMerges);
                            _data[dataPair.Key] = MemoryEntry.Merge(oldEntry, dataPair.Value);
                        }
                    }
                }
                else
                {
                    //copy reference, because its immutable
                    REPORT(Statistic.SimpleHashSearches);
                    _data[dataPair.Key] = dataPair.Value;
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


    }
}
