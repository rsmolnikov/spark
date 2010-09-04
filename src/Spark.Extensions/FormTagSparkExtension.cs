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

    public class FormTagSparkExtension : ISparkExtension
    {
        private OutputStyle outputStyle;
        private bool validate=true;
        private static int _lastFormNumKey = 0;

        public FormTagSparkExtension(ElementNode node)
        {
            m_node = node;
            if (node.Attributes.Contains(new AttributeNode(Constants.VALIDATE_ATTRIBUTE, "true"), new AttributeEqualityComparer()))
                this.outputStyle = OutputStyle.Default;
            else if (node.Attributes.Contains(new AttributeNode(Constants.VALIDATE_ATTRIBUTE, "mvc"), new AttributeEqualityComparer()))
                this.outputStyle = OutputStyle.MVC;
            else validate = false;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                List<Node> newNodes = new List<Node>();

                Utilities.AddApplyPathModifier(m_node.Attributes, "action");
                HttpMethodOverride(body);

                if (validate)
                {

                    AttributeNode idNode = m_node.Attributes.SingleOrDefault(x => x.Name == "id");

                    string id = (idNode != null) ? idNode.Value : GenerateFormId();

                    m_node.Attributes.Remove(m_node.Attributes.FirstOrDefault(x => x.Name == Constants.VALIDATE_ATTRIBUTE));
                    newNodes.Add(m_node);

                    string code = String.Format(Constants.HTML_GETCLIENTVALIDATIONJSON, id, outputStyle);

                    //For MVC we have to create MvcForm first
                    if (outputStyle.Equals(OutputStyle.MVC))
                    {
                        string codeForMvc = String.Format(Constants.CREATEMVCFORM, id);
                        newNodes.Add(new StatementNode(codeForMvc));
                    }
                    newNodes.AddRange(body);
                    newNodes.Add(new StatementNode(code));
                }
                else 
                {
                    newNodes.Add(m_node);
                    newNodes.AddRange(body); 
                }

                newNodes.Add(new EndElementNode(m_node.Name));

                // visit the new nodes normally
                visitor.Accept(newNodes);

                // keep the output chunks to render later
                m_chunks = chunks;
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            visitor.Accept(m_chunks);
        }

        private void HttpMethodOverride(IList<Node> body)
        {
            AttributeNode methodNode = m_node.Attributes.SingleOrDefault(x => x.Name == "method");
            if ((methodNode!=null)&&((methodNode.Value == "put") || (methodNode.Value == "delete")))
            {
                m_node.Attributes.Remove(methodNode);
                m_node.Attributes.Add(new AttributeNode("method", "post"));

                List<AttributeNode> listAttributes = new List<AttributeNode>();
                listAttributes.Add(new AttributeNode("name", "x-http-method-override"));
                listAttributes.Add(new AttributeNode("type", "hidden"));
                listAttributes.Add(new AttributeNode("value", methodNode.Value));
                int indexOfFieldsetNode = 0;

                ElementNode elNode = body.OfType<ElementNode>().FirstOrDefault(x => x.Name == "fieldset");
                if (elNode != null) indexOfFieldsetNode = body.IndexOf(elNode);

                body.Insert(indexOfFieldsetNode + 1, new TextNode(Constants.NEWLINE));
                body.Insert(indexOfFieldsetNode + 2, new ElementNode("input", listAttributes, true));
            }
        }

        private string GenerateFormId()
        {  
            _lastFormNumKey++;
            string id=string.Format(CultureInfo.InvariantCulture, "form_{0}", new object[] { _lastFormNumKey });
            AttributeNode idNode=new AttributeNode("id",id);
            m_node.Attributes.Add(idNode);
            return id;
        }


        private readonly ElementNode m_node;

        private IList<Chunk> m_chunks;

    }
}
