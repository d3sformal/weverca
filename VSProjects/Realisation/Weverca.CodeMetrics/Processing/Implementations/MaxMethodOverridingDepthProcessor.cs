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

            //build an inheritance trees
            foreach (var typeDeclaration in types)
            {
                if (typeDeclaration.BaseClassName.HasValue)
                {
                    AddToInheritanceTrees(types, inheritanceTrees, typeDeclaration);
                }
            }

            List<MethodDecl> occurences = new List<MethodDecl>();

            //find max override depth for each tree
            foreach (var inheritanceTree in inheritanceTrees)
            {
                Dictionary<string, List<MethodDecl>> methods = new Dictionary<string, List<MethodDecl>>();

                Queue<TypeDeclWrapper> queue = new Queue<TypeDeclWrapper>();
                queue.Enqueue(inheritanceTree);

                while (queue.Count > 0)
                {
                    TypeDeclWrapper currentType = queue.Dequeue();

                    //enumerates all methods in the type. The number of the occurences of the same method in the tree is a number of overrides for the method + 1.
                    foreach (var member in currentType.TypeDeclaration.Members)
                    {
                        MethodDecl method = member as MethodDecl;

                        if (method != null)
                        {
                            if (!methods.ContainsKey(method.Name.Value))
                            {
                                methods.Add(method.Name.Value, new List<MethodDecl>());
                            }
                            methods[method.Name.Value].Add(method);
                        }
                    }

                    foreach (var child in currentType.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

                foreach (var currentOccurences in methods.Values)
                {
                    if (currentOccurences.Count > occurences.Count)
                    {
                        occurences.Clear();
                        occurences.AddRange(currentOccurences);
                    }
                }
            }

            // -1 because the first declaration is not overrided.
            return new Result(occurences.Count - 1, occurences);
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
            TypeDecl baseClass = types.First(t => t.Name.Value == newType.BaseClassName.Value.QualifiedName.Name.Value);

            TypeDeclWrapper baseTypeWrapper = FindDTypeWrapper(inheritanceTrees, baseClass);
            TypeDeclWrapper existingTypeWrapper = FindDTypeWrapper(inheritanceTrees, newType);

            if (baseTypeWrapper == null && existingTypeWrapper == null)
            {
                baseTypeWrapper = new TypeDeclWrapper(baseClass);
                baseTypeWrapper.Children.Add(new TypeDeclWrapper(newType));
                inheritanceTrees.Add(baseTypeWrapper);
            }
            else if (baseTypeWrapper != null && existingTypeWrapper == null)
            {
                baseTypeWrapper.Children.Add(existingTypeWrapper);
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
