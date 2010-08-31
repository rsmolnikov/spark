using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark;
using Spark.Parser.Markup;
using Spark.Compiler.NodeVisitors;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using System.Web.Mvc;
using System.Globalization;
using System.Web;
using Spark.Compiler.CSharp.ChunkVisitors;

namespace Spark.Extensions
{

    public class AnchorTagSparkExtension : ISparkExtension
    {
        public AnchorTagSparkExtension(ElementNode node)
        {
            m_node = node;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                List<Node> newNodes = new List<Node>();

                if (HttpContext.Current.Session.IsCookieless)
                {
                    //AddApplyPathModifier for Cookieless
                    AttributeNode hrefNode = m_node.Attributes.SingleOrDefault(x => x.Name == "href");
                    if (hrefNode != null)
                    {
                        AttributeNode newhrefNode = Utilities.AddMethodCallingToAttributeValue(hrefNode, Constants.APPLYAPPPATHMODIFIER);
                        m_node.Attributes.Remove(hrefNode);
                        m_node.Attributes.Add(newhrefNode);
                    }
                }

                newNodes.Add(m_node);
                newNodes.AddRange(body); 
                newNodes.Add(new EndElementNode(m_node.Name));

                // visit the new nodes normally
                visitor.Accept(newNodes);

                // keep the output chunks to render later
                m_chunks = chunks;
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            //when we need to accept chunks? only for GeneratedCodeVisitor or for all?
            if ((visitor is GeneratedCodeVisitor) &&(location == OutputLocation.RenderMethod))
                visitor.Accept(m_chunks);
        }

        private readonly ElementNode m_node;

        private IList<Chunk> m_chunks;

    }
}
