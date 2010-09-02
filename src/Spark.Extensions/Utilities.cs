using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using System.Web;
using Spark.Parser.Markup;
using System.Linq;

namespace Spark.Extensions
{
    public static class Utilities
    {
        internal static AttributeNode AddMethodCallingToAttributeValue(AttributeNode node,string method)
        {
            AttributeNode new_node = new AttributeNode(node.Name, String.Format(method, node.Value));
            new_node.Nodes[0] = new ExpressionNode(String.Format(method, node.Value));
            return new_node;
        }

        internal static void AddApplyPathModifier(IList<AttributeNode> attributes, string name)
        {
            //Response.ApplyAppPathModifier used not only to add cookie, it also resolves urls with ~.
            AttributeNode node = attributes.SingleOrDefault(x => x.Name == name);
            if ((node != null)&&(!node.Value.StartsWith("#")))
            {
                AttributeNode newNode = Utilities.AddMethodCallingToAttributeValue(node, Constants.APPLYAPPPATHMODIFIER);
                attributes.Remove(node);
                attributes.Add(newNode);
            }
        }

        public static string AddBrowserDetails(string value)
        {
            var browser = HttpContext.Current.Request.Browser;
            var cssClass = string.Format("{0} major-{1} minor-{2}", browser.Browser.ToLower(), browser.MajorVersion, browser.MinorVersion);
            if (value != String.Empty) cssClass += " " + value;
            return cssClass;
        }
    }
}
