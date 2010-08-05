using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using System.Web.Mvc;

namespace Spark.Extensions
{
    public class ClientSideValidationSparkExtensionFactory : ISparkExtensionFactory
    {
        public ISparkExtension CreateExtension(VisitorContext context, ElementNode node)
        {  
            if (node.Name.Equals("form"))
            {
                if (node.Attributes.Contains(new AttributeNode(Constants.VALIDATE_ATTRIBUTE, "true"), new AttributeEqualityComparer()))
                    return new ClientSideValidationSparkExtension(node, OutputStyle.Default);
                else 
                    if (node.Attributes.Contains(new AttributeNode(Constants.VALIDATE_ATTRIBUTE, "mvc"), new AttributeEqualityComparer()))
                    return new ClientSideValidationSparkExtension(node, OutputStyle.MVC);
            }
            return null;
        }
    }
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
