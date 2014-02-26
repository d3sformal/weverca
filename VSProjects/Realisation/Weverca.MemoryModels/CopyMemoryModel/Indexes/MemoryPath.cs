using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{

    /// <summary>
    /// Determines in which stack context is has to be looked for the root memory indexes. 
    /// </summary>
    public enum GlobalContext
    {
        /// <summary>
        /// Look only at local level of memory stack
        /// </summary>
        LocalOnly,

        /// <summary>
        /// Look only to the global level of memory stack
        /// </summary>
        GlobalOnly
    }
    
    /// <summary>
    /// Memory path is used in Snapshot Entry object to construct acces path to one or many memory locations
    /// for snapshot entry operations. Path can contain variable name and sequence of fields and indexes
    /// to acces PHP object field or index of PHP associative array.
    /// 
    /// Each part of sequence can contains any number of strings to determine next path segment. Sematics when
    /// segment contains no string is that this is the ANY segment, single string means direct acces and multiple
    /// indicies are used for uncertain indicies which may be accessed.
    /// 
    /// Each path is created in some snapshot entry object and the is processed in some collector algorithm 
    /// to get collection of MemoryIndex objects to determine which locations will be accesed. Collector
    /// algorithm has to handle uncertain segments (any or mulriple names) and inspect location aliases in order to 
    /// traverse memory tree and determine sets of indexes which MUST or MAY be changed.
    /// 
    /// This is imutable class and cannot be changed.
    /// </summary>
    public class MemoryPath
    {
        /// <summary>
        /// Gets the collection of path segments.
        /// </summary>
        /// <value>
        /// The path segments.
        /// </value>
        public ReadOnlyCollection<PathSegment> PathSegments { get; private set; }

        /// <summary>
        /// Gets the length of path.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get { return PathSegments.Count; } }

        /// <summary>
        /// Gets a value indicating whether path is direct (there is no uncertain segment on path).
        /// </summary>
        /// <value>
        ///   <c>true</c> if is direct; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirect { get; private set; }

        /// <summary>
        /// Gets the information whether path starts at local level of memory stack can be in local level identified by CallLevel property.
        /// </summary>
        /// <value>
        /// The global.
        /// </value>
        public GlobalContext Global { get; private set; }

        /// <summary>
        /// Gets the call level of local context where has to be looked for memory locations.
        /// </summary>
        /// <value>
        /// The call level.
        /// </value>
        public int CallLevel { get; private set; }

        #region Path Factories

        /// <summary>
        /// Makes the path which points to single any variable.
        /// </summary>
        /// <param name="global">The global.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New path which points to single any variable</returns>
        public static MemoryPath MakePathAnyVariable(GlobalContext global, int callLevel)
        {
            return new MemoryPath(new VariablePathSegment(), global, callLevel);
        }

        /// <summary>
        /// Makes the path which starts in variables with given names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="global">The global.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New path which starts in variables with given names</returns>
        public static MemoryPath MakePathVariable(IEnumerable<string> names, GlobalContext global, int callLevel)
        {
            return new MemoryPath(new VariablePathSegment(names), global, callLevel);
        }

        /// <summary>
        /// Makes the path to any control variable.
        /// </summary>
        /// <param name="global">The global.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>new path to any control variable</returns>
        public static MemoryPath MakePathAnyControl(GlobalContext global, int callLevel)
        {
            return new MemoryPath(new ControlPathSegment(), global, callLevel);
        }

        /// <summary>
        /// Makes the path to control variables with given names.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="global">The global.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New path to control variables with given names</returns>
        public static MemoryPath MakePathControl(IEnumerable<string> names, GlobalContext global, int callLevel)
        {
            return new MemoryPath(new ControlPathSegment(names), global, callLevel);
        }

        /// <summary>
        /// Makes the path which extends given path by any field.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <returns>New path which extends given path by any field</returns>
        public static MemoryPath MakePathAnyField(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new FieldPathSegment());
        }

        /// <summary>
        /// Makes the path which extends given path by fields with given names.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="names">The names.</param>
        /// <returns>New path which extends given path by fields with given names</returns>
        public static MemoryPath MakePathField(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new FieldPathSegment(names));
        }

        /// <summary>
        /// Makes the path which extends given path by any index.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <returns>New path which extends given path by any index</returns>
        public static MemoryPath MakePathAnyIndex(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new IndexPathSegment());
        }

        /// <summary>
        /// Makes the path which extends given path by indicies with given names.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="names">The names.</param>
        /// <returns>New path which extends given path by indicies with given names</returns>
        public static MemoryPath MakePathIndex(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new IndexPathSegment(names));
        }

        /// <summary>
        /// Makes the path to temporary memory location.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary.</param>
        /// <returns>New path to temporary memory location</returns>
        public static MemoryPath MakePathTemporary(TemporaryIndex temporaryIndex)
        {
            return new MemoryPath(new TemporaryPathSegment(temporaryIndex), GlobalContext.LocalOnly, temporaryIndex.CallLevel);
        }

        #endregion

        /// <summary>
        /// Prevents a default instance of the <see cref="MemoryPath"/> class from being created 
        /// in order to make new path use one of static methods in this class.
        /// 
        /// New path will contain only the root segment.
        /// </summary>
        /// <param name="pathSegment">The path segment.</param>
        /// <param name="global">The global.</param>
        /// <param name="callLevel">The call level.</param>
        private MemoryPath(PathSegment pathSegment, GlobalContext global, int callLevel)
        {
            List<PathSegment> path = new List<PathSegment>();
            path.Add(pathSegment);

            IsDirect = pathSegment.IsDirect;
            Global = global;
            CallLevel = callLevel;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="MemoryPath"/> class from being created
        /// in order to make new path use one of static methods in this class.
        /// 
        /// New path will extends existing path by new segment.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="pathSegment">The path segment.</param>
        private MemoryPath(MemoryPath parentPath, PathSegment pathSegment)
        {
            List<PathSegment> path = new List<PathSegment>(parentPath.PathSegments);
            path.Add(pathSegment);

            IsDirect = parentPath.IsDirect && pathSegment.IsDirect;
            Global = parentPath.Global;
            CallLevel = parentPath.CallLevel;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (CallLevel > Snapshot.GLOBAL_CALL_LEVEL)
            {
                builder.Append(CallLevel + "::");
            }

            foreach (PathSegment name in PathSegments)
            {
                builder.Append(name.ToString());
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Defines visitor to acces path segment.
    /// 
    /// This interface is usually implemented by collector algorithms in order to traverse and process given path by segments.
    /// </summary>
    public interface IPathSegmentVisitor
    {
        /// <summary>
        /// Visits the variable.
        /// </summary>
        /// <param name="variableSegment">The variable segment.</param>
        void VisitVariable(VariablePathSegment variableSegment);

        /// <summary>
        /// Visits the field.
        /// </summary>
        /// <param name="fieldSegment">The field segment.</param>
        void VisitField(FieldPathSegment fieldSegment);

        /// <summary>
        /// Visits the index.
        /// </summary>
        /// <param name="indexSegment">The index segment.</param>
        void VisitIndex(IndexPathSegment indexSegment);

        /// <summary>
        /// Visits the control.
        /// </summary>
        /// <param name="controlPathSegment">The control path segment.</param>
        void VisitControl(ControlPathSegment controlPathSegment);

        /// <summary>
        /// Visits the temporary.
        /// </summary>
        /// <param name="temporaryPathSegment">The temporary path segment.</param>
        void VisitTemporary(TemporaryPathSegment temporaryPathSegment);
    }

    /// <summary>
    /// Base class for path segment which holds set of names for this segment.
    /// 
    /// Derived classes typically adds no functionality (except temporary index). Their main purpose is 
    /// to be visited by IPathSegmentVisitor in order to process each type of segment. They should also
    /// override to string nethod.
    /// </summary>
    public abstract class PathSegment
    {
        /// <summary>
        /// Character which is shown for ANY segments (name of segment is not defined)
        /// </summary>
        public static string UNDEFINED_STR = "?";

        /// <summary>
        /// The possible names for this segment
        /// </summary>
        public readonly ReadOnlyCollection<string> Names;


        /// <summary>
        /// Gets a value indicating whether is any.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is any; otherwise, <c>false</c>.
        /// </value>
        public bool IsAny { get; private set; }


        /// <summary>
        /// Gets a value indicating whether is direct.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is direct; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirect { get; private set; }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public abstract void Accept(IPathSegmentVisitor visitor);

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegment"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public PathSegment(IEnumerable<string> names)
        {
            Names = new ReadOnlyCollection<string>(new List<String>(names));
            IsAny = false;
            IsDirect = Names.Count == 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSegment"/> class.
        /// </summary>
        public PathSegment()
        {
            Names = new ReadOnlyCollection<string>(new List<String>());
            IsAny = true;
            IsDirect = false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return namesToString();
        }

        /// <summary>
        /// Converts stored names to string. This method can be used in child classes to get string represetnation of names.
        /// Possibilities:
        ///  0 names - '?'
        ///  1 name  - 'name'
        ///  x names - '{name1 | name2 | ...}'
        /// </summary>
        /// <returns>String representation of the segment.</returns>
        private string namesToString()
        {
            if (Names.Count == 0)
            {
                return UNDEFINED_STR;
            }
            else if (Names.Count == 1)
            {
                return Names[0];
            }
            else
            {
                StringBuilder builder = new StringBuilder("{");

                builder.Append(Names[0]);

                for (int x = 1; x < Names.Count; x++)
                {
                    builder.Append("|");
                    builder.Append(Names[x]);
                }

                builder.Append("}");
                return builder.ToString();
            }
        }
    }

    /// <summary>
    /// Root segment of acces path which starts in variables.
    /// </summary>
    public class VariablePathSegment : PathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariablePathSegment"/> class.
        /// </summary>
        public VariablePathSegment()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariablePathSegment"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public VariablePathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("${0}", base.ToString());
        }
    }

    /// <summary>
    /// Root segment of acces path which starts in control variables.
    /// </summary>
    public class ControlPathSegment : PathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControlPathSegment"/> class.
        /// </summary>
        public ControlPathSegment()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlPathSegment"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public ControlPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitControl(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("CTRL${0}", base.ToString());
        }
    }

    /// <summary>
    /// Root segment which starts in temporary location specified by given index.
    /// </summary>
    public class TemporaryPathSegment : PathSegment
    {
        /// <summary>
        /// Gets the temporary index where the temporary path belongs to.
        /// </summary>
        /// <value>
        /// The temporary index.
        /// </value>
        public TemporaryIndex TemporaryIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemporaryPathSegment"/> class.
        /// </summary>
        /// <param name="temporaryIndex">Index of the temporary.</param>
        public TemporaryPathSegment(TemporaryIndex temporaryIndex)
        {
            this.TemporaryIndex = temporaryIndex;
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitTemporary(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return TemporaryIndex.ToString();
        }
    }

    /// <summary>
    /// Traversing path segment which continues path in field of PHP object with specified names.
    /// </summary>
    public class FieldPathSegment : PathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPathSegment"/> class.
        /// </summary>
        public FieldPathSegment()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldPathSegment"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public FieldPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitField(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("->{0}", base.ToString());
        }
    }

    /// <summary>
    /// Traversing path segment which continues path in index of associative array with specified names.
    /// </summary>
    public class IndexPathSegment : PathSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexPathSegment"/> class.
        /// </summary>
        public IndexPathSegment()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexPathSegment"/> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public IndexPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitIndex(this);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return String.Format("[{0}]", base.ToString());
        }
    }
}
