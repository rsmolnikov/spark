using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Web.Mvc.Html;

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
        private string _field;
        private List<ModelClientValidationRule> _attributes = new List<ModelClientValidationRule>();

        public string Field
        {
            get { return _field; }
            set { _field = value; }
        }
        public List<ModelClientValidationRule> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

    }
    public static class HtmlHelperExtensions
    {
        public static void GetClientValidationJson(this HtmlHelper htmlHelper, string formID, OutputStyle outputStyle)
        {
            if (outputStyle.Equals(OutputStyle.Default))
            {
                List<ClientValidationField> FieldValidators = new List<ClientValidationField>();
                foreach (ModelMetadata metadata in htmlHelper.ViewData.ModelMetadata.Properties)
                {
                    ClientValidationField field = new ClientValidationField();
                    field.Field = metadata.PropertyName;
                    foreach (ModelClientValidationRule rule in metadata.GetValidators(htmlHelper.ViewContext).SelectMany<ModelValidator, ModelClientValidationRule>(delegate(ModelValidator v)
                    {
                        return v.GetClientValidationRules();
                    }))
                    {
                        field.Attributes.Add(rule);
                    }
                    FieldValidators.Add(field);
                }
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                SortedDictionary<string, object> dictionary = new SortedDictionary<string, object>();
                dictionary.Add("ns", formID);
                dictionary.Add("rules", FieldValidators);

                string jsonValidationMetadata = serializer.Serialize(dictionary);
                string str3 = string.Format(CultureInfo.InvariantCulture, Constants.DEFAULTJSONFORMAT, new object[] { formID, jsonValidationMetadata });
                htmlHelper.ViewContext.Writer.Write(str3);
            }
            else if (outputStyle.Equals(OutputStyle.MVC))
            {
                if (htmlHelper.ViewContext.FormContext != null)
                    htmlHelper.ViewContext.OutputClientValidation();
            }
        }

        public static void GetClientValidationJson(this HtmlHelper htmlHelper, string formID)
        {
            GetClientValidationJson(htmlHelper, formID, OutputStyle.Default);
        }
    }
    
}