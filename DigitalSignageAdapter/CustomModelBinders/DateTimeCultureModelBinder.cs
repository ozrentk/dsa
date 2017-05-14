using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalSignageAdapter.CustomModelBinders
{
    public class DateTimeCultureModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            //controllerContext.HttpContext.Session["cultureInfo"] = cart;
            var langs = controllerContext.HttpContext.Request.UserLanguages;
            CultureInfo ci;

            if (langs == null || langs.Length == 0)
            {
                ci = CultureInfo.InvariantCulture;
            }
            else
            {
                ci = CultureInfo.CreateSpecificCulture(langs[0]);
            }

            //var displayFormat = bindingContext.ModelMetadata.DisplayFormatString;
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            //if (!string.IsNullOrEmpty(displayFormat) && value != null)
            //{
            DateTime date;
            //displayFormat = displayFormat.Replace("{0:", string.Empty).Replace("}", string.Empty);
            // use the format specified in the DisplayFormat attribute to parse the date
            if (DateTime.TryParse(value.AttemptedValue, ci, DateTimeStyles.None, out date))
            {
                return date;
            }
            else
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    string.Format("{0} is an invalid date format", value.AttemptedValue)
                );
            }

            //if (DateTime.TryParseExact(value.AttemptedValue, displayFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            //{
            //    return date;
            //}
            //else
            //{
            //    bindingContext.ModelState.AddModelError(
            //        bindingContext.ModelName,
            //        string.Format("{0} is an invalid date format", value.AttemptedValue)
            //    );
            //}
            //}

            return base.BindModel(controllerContext, bindingContext);
        }
    }
}