using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Configuration;
using System.Web;
using Spark.Parser.Markup;
using System.Linq;
using Spark.Parser.Code;

namespace Spark.Extensions
{
    public static class Utilities
    {
        internal static AttributeNode AddMethodCallingToAttributeValue(AttributeNode node, string method)
        {
            AttributeNode new_node = new AttributeNode(node.Name, String.Format(method, node.Value));

            Snippets snippets = new Snippets();

            Snippet beginMethod = new Snippet();
            beginMethod.Value = method+"(";
            snippets.Add(beginMethod);

            bool needPlus = false;
            foreach (Node child in node.Nodes)
            {
                if (needPlus)
                {
                    Snippet snippet = new Snippet();
                    snippet.Value = "+";
                    snippets.Add(snippet);
                }

                Type typeOfChildNode=child.GetType();
                switch (typeOfChildNode.Name)
                {
                    case ("TextNode"):
                        Snippet snippet=new Snippet();
                        snippet.Value+=String.Format(Constants.STRINGWRAPPER,((TextNode)child).Text);
                        snippets.Add(snippet);
                        break;
                    case ("ExpressionNode"):
                        Snippet hWrapperBegin = new Snippet();
                        hWrapperBegin.Value = "H(";
                        snippets.Add(hWrapperBegin);
                        snippets.AddRange(((ExpressionNode)child).Code);
                        Snippet hWrapperEnd = new Snippet();
                        hWrapperEnd.Value = ")";
                        snippets.Add(hWrapperEnd);
                        break;

                }
                needPlus = true;
            }
            if (node.Nodes.Count == 0)
            {
                Snippet empty = new Snippet();
                empty.Value = String.Format(Constants.STRINGWRAPPER,"");
                snippets.Add(empty);
            }
            Snippet endMethod = new Snippet();
            endMethod.Value = ")";

            snippets.Add(endMethod);

            new_node.Nodes[0] = new ExpressionNode(snippets);
           
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
