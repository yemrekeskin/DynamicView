using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Controllers
{
    public class GenericControllerFactory
        : DefaultControllerFactory
    {
        private IControllerActivator _controllerActivator;
        private IEnumerable<IControllerPropertyActivator> _propertyActivators;

        public GenericControllerFactory(IControllerActivator controllerActivator, IEnumerable<IControllerPropertyActivator> propertyActivators)
            : base(controllerActivator, propertyActivators)
        {
            this._controllerActivator = controllerActivator;
            this._propertyActivators = propertyActivators;
        }

        public override object CreateController(ControllerContext context)
        {
            string controllername = context.RouteData.Values["controller"].ToString();

            Type controllerType = Type.GetType(string.Format("DynamicView.Controllers.{0}Controller", controllername));

            if (controllerType == null)
            {
                var entityType = Type.GetType(string.Format("DynamicView.Models.{0}, DynamicView", controllername));

                if (entityType != null)
                {
                    controllerType = typeof(EntityController<>).MakeGenericType(entityType);
                }
                else
                {
                    // 404
                    return base.CreateController(context);
                }
            }

            Controller controller = Activator.CreateInstance(controllerType) as Controller;
            return controller;
        }

        public override void ReleaseController(ControllerContext context, object controller)
        {
            IDisposable dispose = controller as IDisposable;
            if (dispose != null)
            {
                dispose.Dispose();
            }
        }
    }
}
