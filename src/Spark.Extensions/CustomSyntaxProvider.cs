using System;
using System.Collections.Generic;
using System.Text;
using Spark.Parser.Syntax;
using Spark.Parser;
using Spark.Parser.Markup;
using Spark.Compiler.NodeVisitors;
using Spark.Compiler;
using Spark.Parser.Code;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Spark.Extensions
{
    public class CustomSyntaxProvider : DefaultSyntaxProvider
    {
        private readonly MarkupGrammar _grammar;
        private readonly bool _releaseMode;

        public CustomSyntaxProvider(ISparkSettings settings):base(settings)
        {
            _grammar = new MarkupGrammar(settings);
            _releaseMode = !settings.Debug;
        }

        public override IList<Chunk> GetChunks(VisitorContext context, string path)
        {
            context.SyntaxProvider = this;
            context.ViewPath = path;

            var sourceContext = CreateSourceContext(context.ViewPath, context.ViewFolder);
            var position = new Position(sourceContext);

            var result = _grammar.Nodes(position);
            if (result.Rest.PotentialLength() != 0)
            {
                ThrowParseException(context.ViewPath, position, result.Rest);
            }

            context.Paint = result.Rest.GetPaint();

            var nodes = result.Value;

            //Minification
            if (_releaseMode)
            {
                ((List<Node>)nodes).RemoveAll(x => (x.GetType() == typeof(CommentNode)));
                var nodesToRem = new List<Node>();
                foreach (var textNode in nodes.OfType<TextNode>())
                {
                    var i = nodes.IndexOf(textNode);
                    textNode.Text = Regex.Replace(textNode.Text, "\r\n+|\n+|\t+", "");
                    textNode.Text = Regex.Replace(textNode.Text, "\\s+", " ");
                    //delete node if it is empty or same as previous. (e.g. with one space = " ")
                    if ((String.IsNullOrEmpty(textNode.Text)) || ((i > 0) && (nodes[i - 1].GetType() == typeof(TextNode)) && (((TextNode)nodes[i - 1]).Text == textNode.Text)))
                        nodesToRem.Add(textNode);
                }
                ((List<Node>)nodes).RemoveAll(x => nodesToRem.Contains(x));
            }

            foreach (var visitor in BuildNodeVisitors(context))
            {
                visitor.Accept(nodes);
                nodes = visitor.Nodes;
            }

            var chunkBuilder = new ChunkBuilderVisitor(context);
            chunkBuilder.Accept(nodes);
            return chunkBuilder.Chunks;
        }

        private IList<INodeVisitor> BuildNodeVisitors(VisitorContext context)
        {
            return new INodeVisitor[]
                       {
                           new NamespaceVisitor(context),
                           new IncludeVisitor(context),
                           new PrefixExpandingVisitor(context),
                           new SpecialNodeVisitor(context),
                           new CacheAttributeVisitor(context),
                           new ForEachAttributeVisitor(context),
                           new ConditionalAttributeVisitor(context),
                           new OmitExtraLinesVisitor(context),
                           new TestElseElementVisitor(context),
                           new OnceAttributeVisitor(context),
                           new UrlAttributeVisitor(context),
                           new BindingExpansionVisitor(context)
                       };
        }
    }
}
