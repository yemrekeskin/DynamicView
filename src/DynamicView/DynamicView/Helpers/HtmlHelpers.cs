using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DynamicView.Helpers
{
    //public static class HtmlHelpers
    //{
    //    #region Resources

    //    public static HtmlString LocalizedString(this HtmlHelper helper, string key)
    //    {
    //        return new HtmlString(key);
    //    }

    //    public static MvcHtmlString LocalizedString(this HtmlHelper helper, string key, string cultureCode)
    //    {
    //        return MvcHtmlString.Create(key);
    //    }

    //    public static MvcHtmlString ShowAllErrors(this HtmlHelper helper)
    //    {
    //        var sb = new StringBuilder();

    //        foreach (var modelState in helper.ViewData.ModelState)
    //        {
    //            foreach (var e in modelState.Value.Errors)
    //            {
    //                var div = new TagBuilder("div");
    //                div.MergeAttribute("class", "alert alert-danger");

    //                var button = new TagBuilder("button");
    //                button.MergeAttribute("class", "close");
    //                button.MergeAttribute("data-close", "alert");

    //                var span = new TagBuilder("span");
    //                span.InnerHtml.Append(e.ErrorMessage);

    //                div.InnerHtml = string.Concat(button.ToString(), span.ToString());
    //                sb.Append(div.ToString());
    //            }
    //        }

    //        return MvcHtmlString.Create(sb.ToString());
    //    }

    //    public static MvcHtmlString ShowAllErrors<T>(this HtmlHelper helper)
    //    {
    //        var sb = new StringBuilder();

    //        var propNames = typeof(T).GetProperties().Select(p => p.Name).ToList();
    //        propNames.Insert(0, ""); // Get Global Errors

    //        foreach (var prop in propNames)
    //        {
    //            var modelState = helper.ViewData.ModelState[prop];

    //            if (modelState == null)
    //                continue;

    //            foreach (var e in modelState.Errors)
    //            {
    //                var div = new TagBuilder("div");
    //                div.MergeAttribute("class", "alert alert-danger");

    //                var button = new TagBuilder("button");
    //                button.MergeAttribute("class", "close");
    //                button.MergeAttribute("data-close", "alert");

    //                var span = new TagBuilder("span");
    //                span.InnerHtml.Append(e.ErrorMessage);

    //                div.InnerHtml = string.Concat(button.ToString(), span.ToString());
    //                sb.Append(div.ToString());
    //            }
    //        }

    //        return MvcHtmlString.Create(sb.ToString());
    //    }

    //    public static MvcHtmlString ShowGlobalErrors(this HtmlHelper helper)
    //    {
    //        var sb = new StringBuilder();

    //        var modelState = helper.ViewData.ModelState[string.Empty];

    //        if (modelState == null)
    //        {
    //            if (helper.ViewData.ModelState.Any(p => p.Value != null && p.Value.Errors.Any()))
    //            {
    //                var div = new TagBuilder("div");
    //                div.MergeAttribute("class", "alert alert-danger");

    //                var button = new TagBuilder("button");
    //                button.MergeAttribute("class", "close");
    //                button.MergeAttribute("data-close", "alert");

    //                var span = new TagBuilder("span");
    //                span.InnerHtml.Append("You have some form errors. Please check below.");

    //                div.InnerHtml = string.Concat(button.ToString(), span.ToString());
    //                sb.Append(div.ToString());

    //                return MvcHtmlString.Create(sb.ToString());
    //            }

    //            return null;
    //        }

    //        foreach (var e in modelState.Errors)
    //        {
    //            var div = new TagBuilder("div");
    //            div.MergeAttribute("class", "alert alert-danger");

    //            var button = new TagBuilder("button");
    //            button.MergeAttribute("class", "close");
    //            button.MergeAttribute("data-close", "alert");

    //            var span = new TagBuilder("span");
    //            span.InnerHtml.Append(e.ErrorMessage);

    //            div.InnerHtml = string.Concat(button.ToString(), span.ToString());
    //            sb.Append(div.ToString());
    //        }

    //        return MvcHtmlString.Create(sb.ToString());
    //    }

    //    public static MvcHtmlString SetFieldErrors<T>(this HtmlHelper helper, bool loadOnReady)
    //    {
    //        var sb = new StringBuilder();

    //        var propNames = typeof(T).GetProperties().Select(p => p.Name).ToList();

    //        string invalidProperty = null;

    //        foreach (var prop in propNames)
    //        {
    //            var modelState = helper.ViewData.ModelState[prop];

    //            if (modelState == null || modelState.Errors == null || !modelState.Errors.Any())
    //                continue;

    //            if (invalidProperty == null)
    //                invalidProperty = prop;

    //            foreach (var e in modelState.Errors)
    //            {
    //                sb.Append(string.Format("\"{0}\": \"{1}\",", prop, e.ErrorMessage /*+ "\" tim"*/));
    //            }
    //        }

    //        if (string.IsNullOrEmpty(invalidProperty))
    //            return null;

    //        var scriptSource = string.Format("$('input[name=\"{0}\"]').closest(\"form\").validate().showErrors({{ {1} }});", invalidProperty, sb.ToString().TrimEnd(','));

    //        if (loadOnReady)
    //        {
    //            var script = new TagBuilder("script");
    //            script.MergeAttribute("type", "text-javascript");

    //            script.InnerHtml = string.Format("jQuery(document).ready(function () {{ {0} }});", scriptSource);

    //            return MvcHtmlString.Create(script.ToString());
    //        }

    //        return MvcHtmlString.Create(scriptSource);
    //    }

    //    public static MvcHtmlString SetFieldErrors<T>(this HtmlHelper helper, string formName, bool loadOnReady = false)
    //    {
    //        var sb = new StringBuilder();

    //        var propNames = typeof(T).GetProperties().Select(p => p.Name).ToList();

    //        foreach (var prop in propNames)
    //        {
    //            var modelState = helper.ViewData.ModelState[prop];

    //            if (modelState == null || modelState.Errors == null || !modelState.Errors.Any())
    //                continue;

    //            foreach (var e in modelState.Errors)
    //            {
    //                sb.Append(string.Format("\"{0}\": \"{1}\",", prop, e.ErrorMessage /*+ "\" tim"*/));
    //            }
    //        }

    //        var scriptSource = string.Format("$(\"#{0}\").validate().showErrors({{ {1} }});", formName, sb.ToString().TrimEnd(','));

    //        if (loadOnReady)
    //        {
    //            var script = new TagBuilder("script");
    //            script.MergeAttribute("type", "text-javascript");

    //            script.InnerHtml = string.Format("jQuery(document).ready(function () {{ {0} }});", scriptSource);

    //            return MvcHtmlString.Create(script.ToString());
    //        }

    //        return MvcHtmlString.Create(scriptSource);
    //    }

    //    public static MvcHtmlString SetFieldErrors(this HtmlHelper helper, bool loadOnReady = false)
    //    {
    //        var sb = new StringBuilder();

    //        var setAttr = new StringBuilder();

    //        string invalidProperty = null;

    //        foreach (var modelState in helper.ViewData.ModelState)
    //        {
    //            if (modelState.Key == string.Empty || !modelState.Value.Errors.Any())
    //                continue;

    //            if (invalidProperty == null)
    //                invalidProperty = modelState.Key;

    //            foreach (var e in modelState.Value.Errors)
    //            {
    //                setAttr.Append(string.Format("$('input[name=\"{0}\"]').closest(\".form-group\").attr(\"remote-error\", \"true\"); ",
    //                    modelState.Key));

    //                sb.Append(string.Format("\"{0}\": \"{1}\",", modelState.Key, e.ErrorMessage /*+ "\" tim"*/));
    //            }
    //        }

    //        if (string.IsNullOrEmpty(invalidProperty))
    //            return null;

    //        string scriptSource = string.Format("$('input[name=\"{0}\"]').closest(\"form\").validate().showErrors({{ {1} }}); {2}",
    //            invalidProperty,
    //            sb.ToString().TrimEnd(','),
    //            setAttr.ToString());

    //        if (loadOnReady)
    //        {
    //            var script = new TagBuilder("script");
    //            script.MergeAttribute("type", "text-javascript");

    //            script.InnerHtml = string.Format("jQuery(document).ready(function () {{ {0} }});", scriptSource);

    //            return MvcHtmlString.Create(script.ToString());
    //        }

    //        return MvcHtmlString.Create(scriptSource);
    //    }

    //    public static MvcHtmlString SetFieldErrors(this HtmlHelper helper, string formName, bool loadOnReady = false)
    //    {
    //        var sb = new StringBuilder();

    //        foreach (var modelState in helper.ViewData.ModelState)
    //        {
    //            if (modelState.Key == string.Empty || !modelState.Value.Errors.Any())
    //                continue;

    //            foreach (var e in modelState.Value.Errors)
    //            {
    //                sb.Append(string.Format("\"{0}\": \"{1}\",", modelState.Key, e.ErrorMessage /*+ "\" tim"*/));
    //            }
    //        }

    //        var scriptSource = string.Format("$(\"#{0}\").validate().showErrors({{ {1} }});", formName, sb.ToString().TrimEnd(','));

    //        if (loadOnReady)
    //        {
    //            var script = new TagBuilder("script");
    //            script.MergeAttribute("type", "text-javascript");

    //            script.InnerHtml = string.Format("jQuery(document).ready(function () {{ {0} }});", scriptSource);

    //            return MvcHtmlString.Create(script.ToString());
    //        }

    //        return MvcHtmlString.Create(scriptSource);
    //    }

    //    #endregion
    //}
}