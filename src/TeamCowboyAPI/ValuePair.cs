/*************************************************************************************************
 * Copyright (c) 2018 MagikInfo
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software withou
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/*************************************************************************************************/

namespace MagikInfo.TeamCowboy
{
    using System;

    /// <summary>
    /// A simple class that implements a value pair <string, object>
    /// </summary>
    internal class ValuePair : IComparable
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        /// <summary>
        /// Create a new value pair
        /// </summary>
        /// <param name="name">The name of the pair</param>
        /// <param name="value">The value of the pair</param>
        public ValuePair(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Implementation of the IComparable interface
        /// The comparison is made on the name property
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            return string.Compare(Name, (obj as ValuePair).Name);
        }
    }
}
