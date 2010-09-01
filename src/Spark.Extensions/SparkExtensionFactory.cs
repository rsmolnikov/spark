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
            switch (node.Name)
            {
                case "body":
                    return new BodyTagSparkExtension(node);
                case "form":
                    return new FormTagSparkExtension(node);
                case "a":
                    return new AnchorTagSparkExtension(node);
                default:
                    return null;
            }
        }
    }
    
}
