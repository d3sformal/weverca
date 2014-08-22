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
        /// Determine that member identifier represents any member
        /// </summary>
        public readonly bool IsAny;

        /// <summary>
        /// Determine that member identifier represents statically unknown identifiers
        /// </summary>
        public readonly bool IsUnknown;

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
        private MemberIdentifier(params string[] possibleNames)
            : this((IEnumerable<string>)possibleNames)
        {
        }

        /// <summary>
        /// Creates member identifier for name.
        /// </summary>
        /// <param name="possibleNames">Names of member</param>
        public MemberIdentifier(string possibleNames)
        {
            IsUnknown = false;
            IsAny = false;

            _possibleNames = new string[1];
            _possibleNames[0] = possibleNames;
        }

        public static MemberIdentifier getAnyMemberIdentifier() 
        {
            return new MemberIdentifier (false, true);
        }

        public static MemberIdentifier getUnknownMemberIdentifier()
        {
            return new MemberIdentifier (true, false);
        }

        private MemberIdentifier(bool isUnknown, bool isAny) 
        {
            this.IsUnknown = isUnknown;
            this.IsAny = isAny;
            _possibleNames = new string[0];
        }



        /// <summary>
        /// Creates member identifier for given names.
        /// Names has to be distinct.
        /// </summary>
        /// <param name="possibleNames">Possible names of member</param>
        public MemberIdentifier(IEnumerable<string> possibleNames)
        {
            IsUnknown = false;
            IsAny = false;

            //copy input, because of avoiding further changes
            _possibleNames = possibleNames.ToArray();
        }
    }
}
