using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.Parser;
using Spark.Parser.Syntax;
using Spark.Web.Mvc.Descriptors;
using Spark.Web.Mvc;

namespace Spark.Extensions
{
    public class PrecompiledSupportDescriptorBuilder : DefaultDescriptorBuilder
    {
        private ISparkViewEngine _engine;
        public IList<SparkViewDescriptor> Descriptors { get; set; }

        public PrecompiledSupportDescriptorBuilder():base()  {}

        public PrecompiledSupportDescriptorBuilder(ISparkViewEngine engine) : base(engine){ }

        public PrecompiledSupportDescriptorBuilder(ISparkViewEngine engine, IList<SparkViewDescriptor> descriptors)
            : base(engine) 
        {
            Descriptors = descriptors;
        }

        public override SparkViewDescriptor BuildDescriptor(BuildDescriptorParams buildDescriptorParams, ICollection<string> searchedLocations)
        {
            if (Descriptors != null)
            {
                var viewlocations = PotentialViewLocations(buildDescriptorParams.ControllerName,
                             buildDescriptorParams.ViewName,
                             buildDescriptorParams.Extra);
                return Descriptors.FirstOrDefault(x => (buildDescriptorParams.FindDefaultMaster ? true : x.Templates.Count == 1) && x.Templates.Any(y => viewlocations.ToList().Contains(y)));
            }
            else return base.BuildDescriptor(buildDescriptorParams, searchedLocations);
        }
    }
}
