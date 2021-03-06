﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Globalization;
using System;
using System.Web;
using Spark.Extensions.Validation;

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
        public static IJavaScriptRunner javaScriptRunner { get; set; }

        public static void GetClientValidationJson(this HtmlHelper htmlHelper, string formId, OutputStyle outputStyle, object model)
        {
            if (outputStyle.Equals(OutputStyle.Default))
            {
                var fieldValidators = new Dictionary<string, IList<ModelClientValidationRule>>();
                var viewDataDictionary = (model != null)
                                                            ? new ViewDataDictionary(model)
                                                            : htmlHelper.ViewData;


                foreach (ModelMetadata metadata in viewDataDictionary.ModelMetadata.Properties)
                {
                   // var field = new ClientValidationField {Field = metadata.PropertyName};
                    var rulesList=new List<ModelClientValidationRule>();
                    foreach (var rule in
                        metadata.GetValidators(htmlHelper.ViewContext).SelectMany(modelValidator => modelValidator.GetClientValidationRules()))
                    {
                        rulesList.Add(rule);
                    }
                    if (rulesList.Count!=0)
                        fieldValidators.Add(metadata.PropertyName,rulesList);
                }
                if (javaScriptRunner == null) javaScriptRunner = new JqueryValidateJavaScriptRunner();
                var jsonValidationMetadata = javaScriptRunner.GetClientValidationJson(fieldValidators);
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
            url = url.Trim().Replace("\\", "/") ;
            if (string.IsNullOrEmpty(url))
                return url;
            if (url.StartsWith("/"))
                url=System.String.Format("~{0}", url);
            else if (!url.StartsWith("~"))
                return url;
           return String.IsNullOrEmpty(HttpRuntime.AppDomainAppVirtualPath) ? VirtualPathUtility.ToAbsolute(url, "/") : VirtualPathUtility.ToAbsolute(url);  
        }
    }
    
}