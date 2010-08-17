using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using System.Web;
using Spark.Parser.Markup;

namespace Spark.Extensions
{
    static class Utilities
    {
        internal static AttributeNode AddApplyPathModifier(AttributeNode node)
        {
            if (node != null)
            {
                if (HttpContext.Current.Session.IsCookieless)
                {
                    AttributeNode new_node = new AttributeNode(node.Name, String.Format(Constants.APPLYAPPPATHMODIFIER, node.Value));
                    new_node.Nodes[0] = new ExpressionNode(String.Format(Constants.APPLYAPPPATHMODIFIER, node.Value));
                    return new_node;
                }
                return node;
            }
            return null;
        }
    }
}
