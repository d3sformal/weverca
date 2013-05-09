using System.Collections.Generic;
using Diagnostics = System.Diagnostics;
using System.Linq;

using PHP.Core;
using Weverca.Parsers;
using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Determines the maximum depth of method overriding.
    /// </summary>
    [Metric(Quantity.MaxMethodOverridingDepth)]
    class MaxMethodOverridingDepthProcessor : QuantityProcessor
    {
        #region QualityProcessor overrides

        protected override Result process(bool resolveOccurances, Quantity category, SyntaxParser parser)
        {
            Diagnostics.Debug.Assert(category == Quantity.MaxMethodOverridingDepth);
            Diagnostics.Debug.Assert(parser.IsParsed);
            Diagnostics.Debug.Assert(!parser.Errors.AnyError);

            List<TypeDeclWrapper> inheritanceTrees = new List<TypeDeclWrapper>();

            var types = parser.Types.Values.Select(t => t.Declaration.GetNode() as TypeDecl).Where(t => t != null).AsEnumerable();
            Diagnostics.Debug.Assert(types.Count() == parser.Types.Count);

            //build inheritance trees
            foreach (var typeDeclaration in types)
            {
                if (typeDeclaration.BaseClassName.HasValue)
                {
                    AddToInheritanceTrees(types, inheritanceTrees, typeDeclaration);
                }
            }

            Dictionary<string, List<MethodDecl>> occurences = new Dictionary<string, List<MethodDecl>>();
            Dictionary<string, List<MethodDecl>> methods;

            //find max override depth for each tree
            foreach (var inheritanceTree in inheritanceTrees)
            {
                methods = CheckTree(inheritanceTree);
                MergeResults(methods, occurences);
            }

            List<MethodDecl> maxOccurences = new List<MethodDecl>();
            foreach (var item in occurences.Values)
            {
                if (item.Count > maxOccurences.Count)
                {
                    maxOccurences = item;
                }
            }

            // -1 because the first declaration is not overrided.
            int maxOverrides = maxOccurences.Count - 1;
            if (maxOverrides < 0) // occures when there is no method declared
            {
                maxOverrides = 0;
            }
            return new Result(maxOverrides, maxOccurences);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Finds the type in a set of inheritance trees
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        TypeDeclWrapper FindDTypeWrapper(List<TypeDeclWrapper> types, TypeDecl type)
        {
            foreach (var inheritanceTree in types)
            {
                TypeDeclWrapper result = inheritanceTree.FindType(type);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Adds a type to the correct place in the inheritance trees.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="inheritanceTrees">The inheritance trees.</param>
        /// <param name="newType">The new type.</param>
        void AddToInheritanceTrees(IEnumerable<TypeDecl> types, List<TypeDeclWrapper> inheritanceTrees, TypeDecl newType)
        {
            TypeDecl baseClass = types.FirstOrDefault(t => t.Name.Value == newType.BaseClassName.Value.QualifiedName.Name.Value);
            Diagnostics.Trace.WriteIf(baseClass == null, string.Format("The base type \"{0}\" for \"{1}\" is not available in the source",
                newType.BaseClassName.Value.QualifiedName.Name.Value, newType.Name.Value));

            TypeDeclWrapper baseTypeWrapper = FindDTypeWrapper(inheritanceTrees, baseClass);
            TypeDeclWrapper existingTypeWrapper = FindDTypeWrapper(inheritanceTrees, newType);

            if (baseTypeWrapper == null && existingTypeWrapper == null)
            {
                if (baseClass != null)
                {
                    baseTypeWrapper = new TypeDeclWrapper(baseClass);
                    baseTypeWrapper.Children.Add(new TypeDeclWrapper(newType));
                    inheritanceTrees.Add(baseTypeWrapper);
                }
                else
                {
                    inheritanceTrees.Add(new TypeDeclWrapper(newType));
                }
            }
            else if (baseTypeWrapper != null && existingTypeWrapper == null)
            {
                baseTypeWrapper.Children.Add(new TypeDeclWrapper(newType));
            }
            else if (baseTypeWrapper == null && existingTypeWrapper != null)
            {
                Diagnostics.Debug.Assert(inheritanceTrees.Contains(existingTypeWrapper));

                baseTypeWrapper = new TypeDeclWrapper(baseClass);
                baseTypeWrapper.Children.Add(existingTypeWrapper);
                inheritanceTrees.Remove(existingTypeWrapper);
                inheritanceTrees.Add(baseTypeWrapper);
            }
            else
            {
                Diagnostics.Debug.Fail("Unsupported state");
            }
        }

        /// <summary>
        /// Checks the tree for max overriding depth.
        /// </summary>
        /// <param name="root">The root of the tree of the types to check.</param>
        /// <returns></returns>
        Dictionary<string, List<MethodDecl>> CheckTree(TypeDeclWrapper root)
        {
            Dictionary<string, List<MethodDecl>> result = new Dictionary<string, List<MethodDecl>>();

            foreach (TypeDeclWrapper child in root.Children)
            {
                var childResult = CheckTree(child);
                MergeResults(childResult, result);
            }

            foreach (var member in root.TypeDeclaration.Members)
            {
                MethodDecl method = member as MethodDecl;

                if (method != null)
                {
                    if (!result.ContainsKey(method.Name.Value))
                    {
                        result.Add(method.Name.Value, new List<MethodDecl>());
                    }
                    result[method.Name.Value].Insert(0, method);
                }
            }

            return result;
        }

        /// <summary>
        /// Merges the results of the <see cref="CheckTree"/>.
        /// </summary>
        /// <param name="toMerge">A list of results to merge into the second parameter.</param>
        /// <param name="mergeInto">Result - The first parameter will be merged into this one.</param>
        void MergeResults(Dictionary<string, List<MethodDecl>> toMerge, Dictionary<string, List<MethodDecl>> mergeInto)
        {
            foreach (var item in toMerge)
            {
                if (mergeInto.ContainsKey(item.Key))
                {
                    if (item.Value.Count > mergeInto[item.Key].Count)
                    {
                        mergeInto[item.Key].Clear();
                        mergeInto[item.Key].AddRange(item.Value);
                    }
                }
                else
                {
                    mergeInto.Add(item.Key, item.Value);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Wrapper for the <see cref="TypeDecl"/> to maitain an inheritance tree.
    /// The edges leeds from the base type to derived type.
    /// </summary>
    sealed class TypeDeclWrapper
    {
        #region Properties

        /// <summary>
        /// Gets the base type declaration.
        /// </summary>
        public TypeDecl TypeDeclaration { get; private set; }

        /// <summary>
        /// Gets the derived types.
        /// </summary>
        public List<TypeDeclWrapper> Children { get; private set; }

        #endregion

        #region Constructor

        public TypeDeclWrapper(TypeDecl typeDeclaration)
        {
            TypeDeclaration = typeDeclaration;
            Children = new List<TypeDeclWrapper>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Finds the type in the tree under this instance.
        /// </summary>
        /// <param name="typeDeclaration">The type declaration.</param>
        /// <returns></returns>
        public TypeDeclWrapper FindType(TypeDecl typeDeclaration)
        {
            if (TypeDeclaration == typeDeclaration)
            {
                return this;
            }

            foreach (var child in Children)
            {
                TypeDeclWrapper result = child.FindType(typeDeclaration);
                if (result != null)
                {
                    return child;
                }
            }

            return null;
        }

        #endregion
    }
}
