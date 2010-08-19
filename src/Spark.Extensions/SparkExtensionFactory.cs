using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using System.Web.Mvc;
using System.Web;
using System.Web.Configuration;

namespace Spark.Extensions
{
    public class SparkExtensionFactory : ISparkExtensionFactory
    {
        public ISparkExtension CreateExtension(VisitorContext context, ElementNode node)
        {  
            //what is better: create different extensions for each tags or place all code into one extension?
            if (node.Name.Equals("html"))
                return new HtmlTagSparkExtension(node);
            if (node.Name.Equals("form"))
                return new FormTagSparkExtension(node);
            if (node.Name.Equals("a"))
                return new AnchorTagSparkExtension(node);
            
            return null;
        }
    }
    
}
