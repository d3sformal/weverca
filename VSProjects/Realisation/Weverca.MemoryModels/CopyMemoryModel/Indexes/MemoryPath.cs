using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Represents memory path from variable thru fields and indexes 
    /// Imutable class
    /// </summary>
    class MemoryPath
    {
        public ReadOnlyCollection<PathSegment> PathSegments { get; private set; }
        public int Length { get { return PathSegments.Count; } }

        public bool IsDirect { get; private set; }

        #region Path Factories

        public static MemoryPath MakePathAnyVariable()
        {
            return new MemoryPath(new PathSegment(PathType.Variable));
        }

        public static MemoryPath MakePathVariable(IEnumerable<string> names)
        {
            return new MemoryPath(new PathSegment(names, PathType.Variable));
        }

        public static MemoryPath MakePathAnyField(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new PathSegment(PathType.Field));
        }

        public static MemoryPath MakePathField(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new PathSegment(names, PathType.Field));
        }

        public static MemoryPath MakePathAnyIndex(MemoryPath parentPath)
        {
            return new MemoryPath(parentPath, new PathSegment(PathType.Index));
        }

        public static MemoryPath MakePathIndex(MemoryPath parentPath, IEnumerable<string> names)
        {
            return new MemoryPath(parentPath, new PathSegment(names, PathType.Index));
        }

        #endregion

        private MemoryPath(PathSegment pathSegment)
        {
            List<PathSegment> path = new List<PathSegment>();
            path.Add(pathSegment);

            IsDirect = pathSegment.IsDirect;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

        private MemoryPath(MemoryPath parentPath, PathSegment pathSegment)
        {
            List<PathSegment> path = new List<PathSegment>(parentPath.PathSegments);
            path.Add(pathSegment);

            IsDirect = parentPath.IsDirect && pathSegment.IsDirect;

            PathSegments = new ReadOnlyCollection<PathSegment>(path);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (PathSegment name in PathSegments)
            {
                builder.Append(name.ToString());
            }

            return builder.ToString();
        }
    }

    class PathSegment
    {
        public static string UNDEFINED_STR = "?";

        public readonly ReadOnlyCollection<string> Names;
        public PathType Type { get; private set; }
        public bool IsAny { get; private set; }
        public bool IsDirect { get; private set; }

        public PathSegment(IEnumerable<string> names, PathType type)
        {
            Names = new ReadOnlyCollection<string>(new List<String>(names));
            Type = type;
            IsAny = false;
            IsDirect = Names.Count == 1;
        }

        public PathSegment(PathType type)
        {
            Names = new ReadOnlyCollection<string>(new List<String>());
            Type = type;
            IsAny = true;
            IsDirect = false;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case PathType.Index:
                    return String.Format("[{0}]", namesToString());

                case PathType.Field:
                    return String.Format("->{0}", namesToString());

                default:
                    return namesToString();
            }
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

    enum PathType
    {
        Variable, Field, Index
    }


}
