using System;
using System.Collections.Generic;
using System.Diagnostics;

using PHP.Core.AST;

namespace Weverca.CodeMetrics.Processing.Implementations
{
    /// <summary>
    /// Calculates how many user-defined classes a single class uses on average
    /// </summary>
    [Metric(Rating.ClassCoupling)]
    class ClassCouplingProcessor : RatingProcessor
    {
        #region MetricProcessor overrides

        protected override Result process(bool resolveOccurances, Rating category,
            Parsers.SyntaxParser parser)
        {
            Debug.Assert(category == Rating.ClassCoupling);
            Debug.Assert(parser.IsParsed);

            if (parser.Types == null)
            {
                // No type is declared
                if (resolveOccurances)
                {
                    return new Result(0.0, new DirectTypeRef[0]);
                }
                else
                {
                    return new Result(0.0);
                }
            }

            var types = new Stack<TypeDecl>();
            foreach (var type in parser.Types)
            {
                var node = type.Value.GetNode();
                Debug.Assert(node is TypeDecl);

                var typeNode = node as TypeDecl;
                // As a type, we consider class and interface too
                if ((typeNode.AttributeTarget & PhpAttributeTargets.Types) != 0)
                {
                    types.Push(typeNode);
                }
            }

            var typeReferences = new Stack<KeyValuePair<TypeDecl, DirectTypeRef[]>>();
            foreach (var type in types)
            {
                var visitor = new ClassCouplingVisitor(type.Type);
                type.VisitMe(visitor);
                var references = visitor.GetReferences();
                typeReferences.Push(new KeyValuePair<TypeDecl, DirectTypeRef[]>(
                    type, references));
            }

            var result = CalculateRating(typeReferences.ToArray());

            if (resolveOccurances)
            {
                var allTypeReferences = new Stack<DirectTypeRef>();
                foreach (var typeReference in typeReferences)
                {
                    foreach (var reference in typeReference.Value)
                    {
                        allTypeReferences.Push(reference);
                    }
                }

                // Return all class and interface references (DirectTypeRef) used inside a class declaration
                return new Result(result, allTypeReferences.ToArray());
            }
            else
            {
                return new Result(result);
            }
        }

        /// <summary>
        /// Calculate average number of unique type references that a type contains
        /// </summary>
        /// <returns>Measurement of average number of unique type references inside a type</returns>
        private double CalculateRating(KeyValuePair<TypeDecl, DirectTypeRef[]>[] classCouplings)
        {
            if (classCouplings.Length <= 0)
            {
                return 0.0;
            }

            var numberOfReferences = 0;
            foreach (var classDeclaration in classCouplings)
            {
                Debug.Assert(classDeclaration.Value != null);
                numberOfReferences += classDeclaration.Value.Length;
            }
            return System.Convert.ToDouble(numberOfReferences) / classCouplings.Length;
        }

        #endregion
    }
}
