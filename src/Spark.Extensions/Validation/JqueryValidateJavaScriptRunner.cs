using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Spark.Extensions.Validation
{
    public class JqueryValidateJavaScriptRunner:IJavaScriptRunner
    {
        public string GetClientValidationJson(Dictionary<string, IList<ModelClientValidationRule>> fields)
        {
            var rules = new Dictionary<string, Dictionary<string, object>>();
            var messages = new Dictionary<string, Dictionary<string, object>>();
            foreach(var field in fields)
            {
                var fieldRules = new Dictionary<string, object>();
                var fieldMessages = new Dictionary<string, object>();
                foreach (var fieldRule in field.Value)
                {
                    var c = new ValidationField(fieldRule);
                    foreach (var r in c.rules)
                    {
                        if (!fieldRules.ContainsKey(r.Key))
                            fieldRules.Add(r.Key, r.Value);
                    }
                    foreach (var m in c.messages)
                    {
                        if (!fieldMessages.ContainsKey(m.Key))
                            fieldMessages.Add(m.Key, m.Value);
                    } 
                }
                if (!rules.ContainsKey(field.Key))
                    rules.Add(field.Key, fieldRules);
                if (!messages.ContainsKey(field.Key))
                    messages.Add(field.Key, fieldMessages);
            }
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(new { rules, messages });
        }
        internal class ValidationField
        {
            internal IDictionary<string, object> rules { get; private set; }
            internal IDictionary<string, object> messages { get; private set; }
            internal ValidationField(ModelClientValidationRule fieldRule)
            {
                var self = Populate(fieldRule.ValidationType, fieldRule.ValidationParameters, fieldRule.ErrorMessage);
                rules = self.rules;
                messages = self.messages;
            }

            internal ValidationField(IDictionary<string, object> rules, IDictionary<string, object> messages)
            {
                this.rules = rules;
                this.messages = messages;
            }

            private ValidationField Populate(string ValidationType, IDictionary<string, object> ValidationParameters, string ErrorMessage)
            {
                var fieldRules = new Dictionary<string, object>();
                var fieldMessages = new Dictionary<string, object>();
                switch (ValidationType)
                {
                    case "wrappedRule":
                        var wrappedRules = Populate(Convert.ToString(ValidationParameters["ruleType"]), (IDictionary<string, object>)ValidationParameters["ruleParams"], ErrorMessage);
                        var wrappedRule=new Dictionary<string, object>();
                        wrappedRule.Add("rules",wrappedRules.rules);
                        wrappedRule.Add("expression", ValidationParameters["expression"]);
                        fieldRules.Add("wrapper", wrappedRule);
                        fieldMessages.Add("wrapper", ErrorMessage);
                        break;
                    case "required":
                        fieldRules.Add("required", true);
                        fieldMessages.Add("required", ErrorMessage);
                        break;
                    case "stringLength":
                        fieldRules.Add("minlength", ValidationParameters["minimumLength"]);
                        fieldMessages.Add("minlength", ErrorMessage);
                        fieldRules.Add("maxlength", ValidationParameters["maximumLength"]);
                        fieldMessages.Add("maxlength", ErrorMessage);
                        break;
                    case "range":
                        //TODO: add support for DateRange, if it works... 
                        var ruleName = (bool)ValidationParameters["exclusive"] ? "rangeEx" : "range";
                        var min=ValidationParameters["minimum"];
                        var max=ValidationParameters["maximum"];
                        fieldRules.Add(ruleName, new { min, max });
                        fieldMessages.Add(ruleName, ErrorMessage);
                        break;
                    case "regularExpression":
                        fieldRules.Add("regex", ValidationParameters["pattern"]);
                        fieldMessages.Add("regex", ErrorMessage);
                        break;
                    case "type":
                        var typeName = Convert.ToString(ValidationParameters["typeName"]);
                        fieldRules.Add(typeName, true);
                        fieldMessages.Add(typeName, ErrorMessage);
                        break;
                    case "equalTo":
                        fieldRules.Add("equalTo", ValidationParameters["equalTo"]);
                        fieldMessages.Add("equalTo", ErrorMessage);
                        break;
                }
                return new ValidationField(fieldRules, fieldMessages);
            }
        }     
    }
}
