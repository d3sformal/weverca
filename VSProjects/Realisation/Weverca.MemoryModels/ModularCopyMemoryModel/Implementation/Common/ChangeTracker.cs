using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common
{
    public enum ChangeType 
    {
        Inserted, Deleted, Modified
    }

    public interface IReadonlyChangeTracker<T, C>
    {
        int TrackerId { get; }

        C Container { get; }

        IReadonlyChangeTracker<T, C> PreviousTracker { get; }

        IEnumerable<KeyValuePair<T, ChangeType>> Changes { get; }

        IEnumerable<T> ChangedValues { get; }
    }

    public class ChangeTracker<T, C> : IReadonlyChangeTracker<T, C>
    {
        HashSet<T> changes = new HashSet<T>();

        public int TrackerId { get; private set; }
        public C Container { get; private set; }
        public IReadonlyChangeTracker<T, C> PreviousTracker { get; private set; }
        public IEnumerable<KeyValuePair<T, ChangeType>> Changes { get { return null; } }
        public IEnumerable<T> ChangedValues { get { return changes; } }

        public ChangeTracker(int trackerId, C container, IReadonlyChangeTracker<T, C> previousTracker)
        {
            TrackerId = trackerId;
            Container = container;
            PreviousTracker = previousTracker;
        }

        public void Inserted(T value)
        {
            changes.Add(value);
            //changes[value] = ChangeType.Inserted;
        }

        public void Deleted(T value)
        {
            changes.Add(value);
            //changes[value] = ChangeType.Deleted;
        }

        public void Modified(T value)
        {
            changes.Add(value);
            /*if (!changes.ContainsKey(value))
            {
                changes[value] = ChangeType.Modified;
            }*/
        }
    }
}
