using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Is used as identifier of member (field, index, method) in scope of 
    /// some value (e.g. object, string value, array,..)
    /// 
    /// IS IMMUTABLE
    /// </summary>
    public class MemberIdentifier
    {
        /// <summary>
        /// Possible names of member storage
        /// </summary>
        private readonly string[] _possibleNames;

        /// <summary>
        /// Determine that member identifier name is not known
        /// </summary>
        public bool IsUnknown { get { return NamesCount == 0; } }

        /// <summary>
        /// Determine that member identifier has direct name
        /// </summary>
        public bool IsDirect { get { return DirectName != null; } }


        /// <summary>
        /// Possible names of member
        /// </summary>
        public IEnumerable<string> PossibleNames
        {
            get
            {
                return _possibleNames;
            }
        }

        /// <summary>
        /// Identifier has direct name iff there is exactly one possible name for identifier
        /// If there is more or less than one possible name, null is returned
        /// </summary>
        public string DirectName
        {
            get
            {
                if (NamesCount != 1)
                    return null;

                return _possibleNames[0];
            }
        }

        /// <summary>
        /// Number of possible names for member
        /// </summary>
        public int NamesCount
        {
            get
            {
                return _possibleNames.Length;
            }
        }

        /// <summary>
        /// Creates member identifier for given names.
        /// Names has to be distinct.
        /// </summary>
        /// <param name="possibleNames">Possible names of member</param>
        public MemberIdentifier(IEnumerable<string> possibleNames)
        {
            //copy input, because of avoiding further changes
            _possibleNames = possibleNames.ToArray();
        }
    }
}
