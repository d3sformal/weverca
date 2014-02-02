using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    enum GlobalContext
    {
        LocalOnly, GlobalOnly
    }
    
    /// <summary>
    /// Represents memory path from variable thru fields and indexes 
    /// Imutable class
    /// </summary>
    class MemoryPath
    {
        public ReadOnlyCollection<PathSegment> PathSegments { get; private set; }
        public int Length { get { return PathSegments.Count; } }

        public bool IsDirect { get; private set; }
        public GlobalContext Global { get; private set; }
        public int CallLevel { get; private set; }

        #region Path Factories

        public static MemoryPath MakePathAnyVariable(GlobalContext global, int callLevel)
        {
            return new MemoryPath(new VariablePathSegment(), global, callLevel);
        }

        public static MemoryPath MakePathVariable(IEnumerable<string> names, GlobalContext global, int callLevel)
        {
            return new MemoryPath(new VariablePathSegment(names), global, callLevel);
        }

        public static MemoryPath MakePathAnyControl(GlobalContext global, int callLevel)
        {
            return new MemoryPath(new ControlPathSegment(), global, callLevel);
        }

        public static MemoryPath MakePathControl(IEnumerable<string> names, GlobalContext global, int callLevel)
        {
            return new MemoryPath(new ControlPathSegment(names), global, callLevel);
        }

        public static MemoryPath MakePathAnyField(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new FieldPathSegment());
        }

        public static MemoryPath MakePathField(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new FieldPathSegment(names));
        }

        public static MemoryPath MakePathAnyIndex(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new IndexPathSegment());
        }

        public static MemoryPath MakePathIndex(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new IndexPathSegment(names));
        }

        public static MemoryPath MakePathTemporary(TemporaryIndex temporaryIndex)
        {
            return new MemoryPath(new TemporaryPathSegment(temporaryIndex), GlobalContext.LocalOnly, temporaryIndex.CallLevel);
        }

        #endregion

        private MemoryPath(PathSegment pathSegment, GlobalContext global, int callLevel)
        {
            List<PathSegment> path = new List<PathSegment>();
            path.Add(pathSegment);

            IsDirect = pathSegment.IsDirect;
            Global = global;
            CallLevel = callLevel;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

        private MemoryPath(MemoryPath parentPath, PathSegment pathSegment)
        {
            List<PathSegment> path = new List<PathSegment>(parentPath.PathSegments);
            path.Add(pathSegment);

            IsDirect = parentPath.IsDirect && pathSegment.IsDirect;
            Global = parentPath.Global;
            CallLevel = parentPath.CallLevel;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

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

    public interface IPathSegmentVisitor
    {
        void VisitVariable(VariablePathSegment variableSegment);

        void VisitField(FieldPathSegment fieldSegment);

        void VisitIndex(IndexPathSegment indexSegment);

        void VisitControl(ControlPathSegment controlPathSegment);

        void VisitTemporary(TemporaryPathSegment temporaryPathSegment);
    }

    public abstract class PathSegment
    {
        public static string UNDEFINED_STR = "?";

        public readonly ReadOnlyCollection<string> Names;
        public bool IsAny { get; private set; }
        public bool IsDirect { get; private set; }

        public abstract void Accept(IPathSegmentVisitor visitor);

        public PathSegment(IEnumerable<string> names)
        {
            Names = new ReadOnlyCollection<string>(new List<String>(names));
            IsAny = false;
            IsDirect = Names.Count == 1;
        }

        public PathSegment()
        {
            Names = new ReadOnlyCollection<string>(new List<String>());
            IsAny = true;
            IsDirect = false;
        }

        public override string ToString()
        {
            return namesToString();
        }

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



    public class VariablePathSegment : PathSegment
    {
        public VariablePathSegment()
            : base()
        {

        }

        public VariablePathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        public override string ToString()
        {
            return String.Format("${0}", base.ToString());
        }
    }

    public class ControlPathSegment : PathSegment
    {
        public ControlPathSegment()
            : base()
        {

        }

        public ControlPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitControl(this);
        }

        public override string ToString()
        {
            return String.Format("CTRL${0}", base.ToString());
        }
    }

    public class TemporaryPathSegment : PathSegment
    {
        public TemporaryIndex TemporaryIndex { get; private set; }

        public TemporaryPathSegment(TemporaryIndex temporaryIndex)
        {
            this.TemporaryIndex = temporaryIndex;
        }

        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitTemporary(this);
        }

        public override string ToString()
        {
            return TemporaryIndex.ToString();
        }
    }



    public class FieldPathSegment : PathSegment
    {
        public FieldPathSegment()
            : base()
        {

        }

        public FieldPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitField(this);
        }

        public override string ToString()
        {
            return String.Format("->{0}", base.ToString());
        }
    }

    public class IndexPathSegment : PathSegment
    {
        public IndexPathSegment()
            : base()
        {

        }

        public IndexPathSegment(IEnumerable<string> names)
            : base(names)
        {

        }

        public override void Accept(IPathSegmentVisitor visitor)
        {
            visitor.VisitIndex(this);
        }

        public override string ToString()
        {
            return String.Format("[{0}]", base.ToString());
        }
    }
}
