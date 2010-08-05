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

namespace Spark.Extensions
{

    public class ClientSideValidationSparkExtension : ISparkExtension
    {
        private OutputStyle outputStyle;
        private static int _lastFormNumKey = 0;

        public ClientSideValidationSparkExtension(ElementNode node, OutputStyle outputStyle)
        {
            m_node = node;
            this.outputStyle = outputStyle;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                List<Node> newNodes = new List<Node>();
                AttributeNode idNode = m_node.Attributes.SingleOrDefault(x => x.Name == "id");
                 
                string id = (idNode != null) ? idNode.Value : GenerateFormId();
               
                m_node.Attributes.Remove(m_node.Attributes.First(x => x.Name == Constants.VALIDATE_ATTRIBUTE));
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
                newNodes.Add(new EndElementNode(m_node.Name));
              
                // visit the new nodes normally
                visitor.Accept(newNodes);

                // keep the output chunks to render later
                m_chunks = chunks;
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {

            if (location == OutputLocation.RenderMethod)
                visitor.Accept(m_chunks);
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
