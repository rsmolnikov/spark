using System;
using System.Collections.Generic;
using System.Text;
using Spark.Parser.Markup;

namespace Spark.Extensions
{
    internal class AttributeEqualityComparer : IEqualityComparer<AttributeNode>
    {
        public bool Equals(AttributeNode b1, AttributeNode b2)
        {
            if (b1.Name == b2.Name && b1.Value == b2.Value)
                return true;
            else
                return false;
        }

        public int GetHashCode(AttributeNode bx)
        {
            return base.GetHashCode();
        }

    }
}
