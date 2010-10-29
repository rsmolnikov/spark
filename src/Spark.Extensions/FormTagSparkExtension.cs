using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;
using Spark.Compiler.NodeVisitors;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using System.Globalization;

namespace Spark.Extensions
{

    public class FormTagSparkExtension : ISparkExtension
    {
        private OutputStyle outputStyle;
        private bool validate=true;
        private static int _lastFormNumKey = 0;
        private string modelName="null";

        public FormTagSparkExtension(ElementNode node)
        {
            _mNode = node; 
            var attributeNode = node.Attributes.SingleOrDefault(x => x.Name == Constants.VALIDATE_ATTRIBUTE);
            if (attributeNode != null)
            {
                var attributeNodeValue = attributeNode.Value.ToLower();
                if (attributeNodeValue == "mvc")
                    this.outputStyle = OutputStyle.MVC;
                else 
                {
                    this.outputStyle = OutputStyle.Default;
                    if (attributeNodeValue != "true")
                        modelName = attributeNode.Value;
                }
            }
            else validate = false;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                var newNodes = new List<Node>();

                Utilities.AddApplyPathModifier(_mNode.Attributes, "action");
                HttpMethodOverride(body);

                if (validate)
                {

                    var idNode = _mNode.Attributes.SingleOrDefault(x => x.Name == "id");

                    var id = (idNode != null) ? idNode.Value : GenerateFormId();

                    _mNode.Attributes.Remove(_mNode.Attributes.FirstOrDefault(x => x.Name == Constants.VALIDATE_ATTRIBUTE));
                    newNodes.Add(_mNode);

                    var code = String.Format(Constants.HTML_GETCLIENTVALIDATIONJSON, id, outputStyle, modelName);

                    //For MVC we have to create MvcForm first
                    if (outputStyle.Equals(OutputStyle.MVC))
                    {
                        var codeForMvc = String.Format(Constants.CREATEMVCFORM, id);
                        newNodes.Add(new StatementNode(codeForMvc));
                    }
                    newNodes.AddRange(body);
                    newNodes.Add(new StatementNode(code));
                }
                else 
                {
                    newNodes.Add(_mNode);
                    newNodes.AddRange(body); 
                }

                newNodes.Add(new EndElementNode(_mNode.Name));

                // visit the new nodes normally
                visitor.Accept(newNodes);

                // keep the output chunks to render later
                _mChunks = chunks;
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            visitor.Accept(_mChunks);
        }

        private void HttpMethodOverride(IList<Node> body)
        {
            var methodNode = _mNode.Attributes.SingleOrDefault(x => x.Name == "method");
            if ((methodNode!=null)&&((methodNode.Value == "put") || (methodNode.Value == "delete")))
            {
                _mNode.Attributes.Remove(methodNode);
                _mNode.Attributes.Add(new AttributeNode("method", "post"));

                var listAttributes = new List<AttributeNode>();
                listAttributes.Add(new AttributeNode("name", "x-http-method-override"));
                listAttributes.Add(new AttributeNode("type", "hidden"));
                listAttributes.Add(new AttributeNode("value", methodNode.Value));
                int indexOfFieldsetNode = 0;

                var elNode = body.OfType<ElementNode>().FirstOrDefault(x => x.Name == "fieldset");
                if (elNode != null) indexOfFieldsetNode = body.IndexOf(elNode);

                body.Insert(indexOfFieldsetNode + 2, new ElementNode("input", listAttributes, true));
            }
        }

        private string GenerateFormId()
        {  
            _lastFormNumKey++;
            string id=string.Format(CultureInfo.InvariantCulture, "form_{0}", new object[] { _lastFormNumKey });
            var idNode=new AttributeNode("id",id);
            _mNode.Attributes.Add(idNode);
            return id;
        }


        private readonly ElementNode _mNode;

        private IList<Chunk> _mChunks;

    }
}
