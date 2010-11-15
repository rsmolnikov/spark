using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Globalization;

namespace Spark.Extensions
{
    public enum OutputStyle
    {
        Default,
        MVC
    }

    //class for JavaScript serialization
    internal class ClientValidationField
    {
        private List<ModelClientValidationRule> _attributes = new List<ModelClientValidationRule>();

        public string Field { get; set; }

        public List<ModelClientValidationRule> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

    }
    public static class HtmlHelperExtensions
    {
        public static void GetClientValidationJson(this HtmlHelper htmlHelper, string formId, OutputStyle outputStyle, object model)
        {
            if (outputStyle.Equals(OutputStyle.Default))
            {
                var fieldValidators = new List<ClientValidationField>();
                var viewDataDictionary = (model != null)
                                                            ? new ViewDataDictionary(model)
                                                            : htmlHelper.ViewData;


                foreach (ModelMetadata metadata in viewDataDictionary.ModelMetadata.Properties)
                {
                    var field = new ClientValidationField {Field = metadata.PropertyName};
                    foreach (var rule in
                        metadata.GetValidators(htmlHelper.ViewContext).SelectMany(modelValidator => modelValidator.GetClientValidationRules()))
                    {
                        field.Attributes.Add(rule);
                    }
                    if (field.Attributes.Count!=0)
                        fieldValidators.Add(field);
                }
                var serializer = new JavaScriptSerializer();
                var dictionary = new SortedDictionary<string, object> {{"ns", formId}, {"rules", fieldValidators}};

                var jsonValidationMetadata = serializer.Serialize(dictionary);
                var str3 = string.Format(CultureInfo.InvariantCulture, Constants.DEFAULTJSONFORMAT, new object[] { formId, jsonValidationMetadata });
                htmlHelper.ViewContext.Writer.Write(str3);
            }
            else if (outputStyle.Equals(OutputStyle.MVC))
            {
                if (htmlHelper.ViewContext.FormContext != null)
                    htmlHelper.ViewContext.OutputClientValidation();
            }
        }

        public static void GetClientValidationJson(this HtmlHelper htmlHelper, string formId)
        {
            GetClientValidationJson(htmlHelper, formId, OutputStyle.Default, null);
        }

        public static string ToApplicationRelativeUrl(this HtmlHelper htmlHelper, string url)
        {
            url=url.Trim();
            if (string.IsNullOrEmpty(url))
                return url;
            if (url.StartsWith("/"))
                url=System.String.Format("~{0}", url);
            else if (!url.StartsWith("~"))
                return url;
            return System.Web.VirtualPathUtility.ToAbsolute(url);         
        }
    }
    
}