using System;
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using PHP.Core.Reflection;
using Weverca.Parsers;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    [Metric(Quantity.MaxMethodOverridingDepth)]
    class MaxMethodOverridingDepthProcessor : QuantityProcessor
    {
        #region QualityProcessor overrides

        protected override Result process(bool resolveOccurances, Quantity category, SyntaxParser parser)
        {
            Diagnostics.Debug.Assert(category == Quantity.MaxMethodOverridingDepth);
            Diagnostics.Debug.Assert(parser.IsParsed);
            Diagnostics.Debug.Assert(!parser.Errors.AnyError);

            List<PhpTypeWrapper> inheritanceTrees = new List<PhpTypeWrapper>();

            foreach (var classDeclaration in parser.Types.Values)
            {
                TypeDecl typeDecl = classDeclaration.Declaration.GetNode() as TypeDecl;
                var baseClassName = typeDecl.BaseClassName;

                if (baseClassName.HasValue)
                {
                    AddToInheritanceTrees(parser.Types.Values, inheritanceTrees, classDeclaration);
                }
            }

            int maxDepth = 0;

            foreach (var inheritanceTree in inheritanceTrees)
            {
                Dictionary<string, int> methods = new Dictionary<string, int>();

                Queue<PhpTypeWrapper> queue = new Queue<PhpTypeWrapper>();
                queue.Enqueue(inheritanceTree);

                while (queue.Count > 0)
                {
                    PhpTypeWrapper currentType = queue.Dequeue();

                    TypeDecl currentTypeDeclaration = currentType.PhpType.Declaration.GetNode() as TypeDecl;

                    foreach (var currentMethod in currentTypeDeclaration.Members)
                    {
                        MethodDecl method = currentMethod as MethodDecl;

                        if (method != null && !methods.ContainsKey(method.Name.Value))
                        {
                            methods.Add(method.Name.Value, -1); // will be incremented shortely
                        }
                        methods[method.Name.Value]++;
                    }

                    foreach (var child in currentType.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

                foreach (var depth in methods.Values)
                {
                    if (depth > maxDepth)
                    {
                        maxDepth = depth;
                    }
                }
            }

            return new Result(maxDepth);
        }

        #endregion

        #region Private Methods

        PhpTypeWrapper FindDTypeWrapper(List<PhpTypeWrapper> types, PhpType type)
        {
            foreach (var inheritanceTree in types)
            {
                PhpTypeWrapper result = inheritanceTree.FindType(type);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        void AddToInheritanceTrees(IEnumerable<PhpType> types, List<PhpTypeWrapper> inheritanceTrees, PhpType newType)
        {
            TypeDecl newTypeDecl = newType.Declaration.GetNode() as TypeDecl;
            PhpType baseClass = types.First(t => t.FullName == newTypeDecl.BaseClassName.Value.QualifiedName.Name.Value);

            PhpTypeWrapper baseTypeWrapper = FindDTypeWrapper(inheritanceTrees, baseClass);
            PhpTypeWrapper existingTypeWrapper = FindDTypeWrapper(inheritanceTrees, newType);

            if (baseTypeWrapper == null && existingTypeWrapper == null)
            {
                baseTypeWrapper = new PhpTypeWrapper(baseClass);
                baseTypeWrapper.Children.Add(new PhpTypeWrapper(newType));
                inheritanceTrees.Add(baseTypeWrapper);
            }
            else if (baseTypeWrapper != null && existingTypeWrapper == null)
            {
                baseTypeWrapper.Children.Add(existingTypeWrapper);
            }
            else if (baseTypeWrapper == null && existingTypeWrapper != null)
            {
                Debug.Assert(inheritanceTrees.Contains(existingTypeWrapper));

                baseTypeWrapper = new PhpTypeWrapper(baseClass);
                baseTypeWrapper.Children.Add(existingTypeWrapper);
                inheritanceTrees.Remove(existingTypeWrapper);
                inheritanceTrees.Add(baseTypeWrapper);
            }
            else
            {
                Debug.Fail("Unsupported state");
            }
        }

        #endregion
    }

    class PhpTypeWrapper
    {
        public PhpType PhpType { get; private set; }
        public List<PhpTypeWrapper> Children { get; private set; }

        public PhpTypeWrapper(PhpType phpType)
        {
            PhpType = phpType;
            Children = new List<PhpTypeWrapper>();
        }

        public PhpTypeWrapper FindType(PhpType type)
        {
            if (PhpType == type)
            {
                return this;
            }

            foreach (var child in Children)
            {
                PhpTypeWrapper result = child.FindType(type);
                if (result != null)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
