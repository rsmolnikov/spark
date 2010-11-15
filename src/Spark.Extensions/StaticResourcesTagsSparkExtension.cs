using System.Collections.Generic;
using System.Text;
using Spark.Parser.Markup;
using Spark.Compiler.NodeVisitors;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Extensions
{
    //for link, script and img tags. In future they may will be separated.
    public class StaticResourcesTagsSparkExtension : ISparkExtension
    {
        private string _linkattribute;
        public StaticResourcesTagsSparkExtension(ElementNode node, string linkAttribute)
        {
            _mNode = node;
            _linkattribute = linkAttribute;
        }

        public void VisitNode(INodeVisitor visitor, IList<Node> body, IList<Chunk> chunks)
        {
            if (visitor is ChunkBuilderVisitor)
            {
                var newNodes = new List<Node>();

                Utilities.ToApplicationRelativeUrl(_mNode.Attributes, _linkattribute);
                
                newNodes.Add(_mNode);
                if (!_mNode.IsEmptyElement)
                {
                    newNodes.AddRange(body);
                    newNodes.Add(new EndElementNode(_mNode.Name));
                }

                // visit the new nodes normally
                visitor.Accept(newNodes);

                // keep the output chunks to render later
                _mChunks = chunks;
            }
        }

        public void VisitChunk(IChunkVisitor visitor, OutputLocation location, IList<Chunk> body, StringBuilder output)
        {
            //when we need to accept chunks?
            if (location == OutputLocation.RenderMethod)
                visitor.Accept(_mChunks);
        }

        private readonly ElementNode _mNode;

        private IList<Chunk> _mChunks;

    }
}
