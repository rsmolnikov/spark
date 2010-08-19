using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using System.Web;
using Spark.Parser.Markup;

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
        public static string AddBrowserDetails(string value)
        {
            var browser = HttpContext.Current.Request.Browser;
            var cssClass = string.Format("{0} major-{1} minor-{2}", browser.Browser.ToLower(), browser.MajorVersion, browser.MinorVersion);
            if (value != String.Empty) cssClass += " " + value;
            return cssClass;
        }
    }
}
