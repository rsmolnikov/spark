using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Spark.Parser.Markup;
using System.Linq;
using Spark.Parser.Code;
using Spark.Compiler;

namespace Spark.Extensions
{
    public static class Utilities
    {
        internal static AttributeNode AddMethodCallingToAttributeValue(AttributeNode node, string method)
        {
            Snippets snippets = new Snippets();
            snippets.Add(new Snippet(){Value= method+"(H("});
            snippets.AddRange(AttributeNodeExtensions.AsCodeInverted(node));
            snippets.Add(new Snippet(){Value= "))"});

            ExpressionBuilder builder = new ExpressionBuilder();
            builder.AppendExpression(snippets);
            List<Node> listNodes=new List<Node>();
            listNodes.Add(new ExpressionNode(snippets));
            return new AttributeNode(node.Name, listNodes);
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
