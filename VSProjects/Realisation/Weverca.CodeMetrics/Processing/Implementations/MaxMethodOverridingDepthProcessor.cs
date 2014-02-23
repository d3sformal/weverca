using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using PHP.Core.AST;

using Weverca.Parsers;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Determines the maximum depth of method overriding.
    /// The maximum depth is in this case the maximum distance of the method declaration and the farthest override in the tree of inheritance.
    /// </summary>
    [Metric(Quantity.MaxMethodOverridingDepth)]
    internal class MaxMethodOverridingDepthProcessor : QuantityProcessor
    {
        #region MetricProcessor overrides

        /// <inheritdoc />
        public override Result Process(bool resolveOccurances, Quantity category,
            SyntaxParser parser)
        {
            Debug.Assert(category == Quantity.MaxMethodOverridingDepth,
                "Metric of class must be same as passed metric");
            Debug.Assert(parser.IsParsed, "Source code must be parsed");
            Debug.Assert(!parser.Errors.AnyError, "Source code must not have any syntax error");

            var types = new Queue<TypeDecl>();

            foreach (var type in parser.Types)
            {
                var node = type.Value.Declaration.GetNode();
                var typeDeclaration = node as TypeDecl;
                Debug.Assert(typeDeclaration != null, "PhpType is always in type declaration node");

                types.Enqueue(typeDeclaration);
            }

            var inheritanceTrees = new List<TypeDeclWrapper>();

            // Build inheritance trees
            foreach (var typeDeclaration in types)
            {
                if (typeDeclaration.BaseClassName.HasValue)
                {
                    AddToInheritanceTrees(types, inheritanceTrees, typeDeclaration);
                }
            }

            var occurences = new Dictionary<string, List<MethodDecl>>();
            Dictionary<string, List<MethodDecl>> methods;

            // Find max override depth for each tree
            foreach (var inheritanceTree in inheritanceTrees)
            {
                methods = CheckTree(inheritanceTree);
                MergeResults(methods, occurences);
            }

            var maxOccurences = new List<MethodDecl>();
            foreach (var item in occurences.Values)
            {
                if (item.Count > maxOccurences.Count)
                {
                    maxOccurences = item;
                }
            }

            // -1 because the first declaration is not overrided.
            int maxOverrides = maxOccurences.Count - 1;
            if (maxOverrides < 0)
            {
                // It occures when there is no method declared
                maxOverrides = 0;
            }

            return new Result(maxOverrides, maxOccurences);
        }

        #endregion MetricProcessor overrides

        #region Private Methods

        /// <summary>
        /// Finds the type in a set of inheritance trees.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static TypeDeclWrapper FindDTypeWrapper(List<TypeDeclWrapper> types, TypeDecl type)
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
        private static void AddToInheritanceTrees(IEnumerable<TypeDecl> types,
            List<TypeDeclWrapper> inheritanceTrees, TypeDecl newType)
        {
            var baseClassName = newType.BaseClassName.Value.QualifiedName.Name.Value;
            TypeDecl baseClass = null;

            foreach (var type in types)
            {
                if (type.Name.Value.Equals(baseClassName, StringComparison.Ordinal))
                {
                    baseClass = type;
                    break;
                }
            }

            var message = string.Format(CultureInfo.InvariantCulture,
                "The base type \"{0}\" for \"{1}\" is not available in the source",
                baseClassName, newType.Name.Value);
            Trace.WriteIf(baseClass == null, message);

            var baseTypeWrapper = FindDTypeWrapper(inheritanceTrees, baseClass);
            var existingTypeWrapper = FindDTypeWrapper(inheritanceTrees, newType);

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
                Debug.Assert(inheritanceTrees.Contains(existingTypeWrapper));

                baseTypeWrapper = new TypeDeclWrapper(baseClass);
                baseTypeWrapper.Children.Add(existingTypeWrapper);
                inheritanceTrees.Remove(existingTypeWrapper);
                inheritanceTrees.Add(baseTypeWrapper);
            }
            else
            {
                Debug.Fail("Unsupported state");
            }
        }

        /// <summary>
        /// Checks the tree for max overriding depth.
        /// </summary>
        /// <param name="root">The root of the tree of the types to check.</param>
        /// <returns></returns>
        private Dictionary<string, List<MethodDecl>> CheckTree(TypeDeclWrapper root)
        {
            Dictionary<string, List<MethodDecl>> result = new Dictionary<string, List<MethodDecl>>();

            foreach (TypeDeclWrapper child in root.Children)
            {
                var childResult = CheckTree(child);
                MergeResults(childResult, result);
            }

            foreach (var member in root.TypeDeclaration.Members)
            {
                var method = member as MethodDecl;
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
        private static void MergeResults(Dictionary<string, List<MethodDecl>> toMerge,
            Dictionary<string, List<MethodDecl>> mergeInto)
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

        #endregion Private Methods
    }

    /// <summary>
    /// Wrapper for the <see cref="TypeDecl"/> to maintain an inheritance tree.
    /// The edges leeds from the base type to derived type.
    /// </summary>
    internal sealed class TypeDeclWrapper
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

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDeclWrapper" /> class.
        /// </summary>
        /// <param name="typeDeclaration"></param>
        public TypeDeclWrapper(TypeDecl typeDeclaration)
        {
            TypeDeclaration = typeDeclaration;
            Children = new List<TypeDeclWrapper>();
        }

        #endregion Constructor

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

        #endregion Methods
    }
}
