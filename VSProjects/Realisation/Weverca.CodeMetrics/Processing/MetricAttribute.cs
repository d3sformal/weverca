/*
Copyright (c) 2012-2014 Miroslav Vodolan, Matyas Brenner, David Skorvaga, David Hauzar.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;

namespace Weverca.CodeMetrics.Processing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class MetricAttribute : Attribute
    {
        private ConstructIndicator[] indicators;

        private Rating[] ratings;

        private Quantity[] quantities;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="indicators"></param>
        public MetricAttribute(params ConstructIndicator[] indicators)
        {
            this.indicators = indicators;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="ratings"></param>
        public MetricAttribute(params Rating[] ratings)
        {
            this.ratings = ratings;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricAttribute" /> class.
        /// </summary>
        /// <param name="quantities"></param>
        public MetricAttribute(params Quantity[] quantities)
        {
            this.quantities = quantities;
        }
    }
}