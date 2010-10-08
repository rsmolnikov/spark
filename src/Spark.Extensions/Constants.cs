namespace Spark.Extensions
{
    public abstract class Constants
    {
        public const string CREATEMVCFORM = "Html.EnableClientValidation(); new MvcForm(Html.ViewContext); Html.ViewContext.FormContext.FormId=\"{0}\";";
        public const string HTML_GETCLIENTVALIDATIONJSON = "Html.GetClientValidationJson(\"{0}\", OutputStyle.{1}, {2});";
        public const string DEFAULTJSONFORMAT = "\r\n<script type=\"text/javascript\">\r\nif (clientValidation == undefined) var clientValidation = new Array(); clientValidation[\"{0}\"] =  {1};\r\n</script>";
        public const string VALIDATE_ATTRIBUTE = "validate";
        public const string VALIDATEMODEL_ATTRIBUTE = "validatemodel";
        public const string APPLYAPPPATHMODIFIER = "Response.ApplyAppPathModifier";
        public const string ADDBROWSERDETAILS = "Utilities.AddBrowserDetails";
        public const string NEWLINE = "\r\n  ";
    }
}
