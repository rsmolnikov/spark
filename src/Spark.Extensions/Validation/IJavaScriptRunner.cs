using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spark.Extensions.Validation
{
    public interface IJavaScriptRunner
    {
        string GetClientValidationJson(Dictionary<string, IList<ModelClientValidationRule>> fields);
    }
}
