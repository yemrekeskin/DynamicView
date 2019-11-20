using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DynamicView.Attributes;
using DynamicView.Context;
using DynamicView.Entity;
using DynamicView.Models;
using DynamicView.Utils;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamicView.Controllers
{
    public class EntityController<T> : BaseController
         where T : class, IEntity, new()    
    {
        public IActionResult Index()
        {
            return View();
        }

        #region Actions

        [HttpGet]
        public virtual ActionResult List()
        {
            var entityType = typeof(T);

            var listPageOutModel = GetListPageOutModel(entityType);

            ViewBag.Title = listPageOutModel.MetaData.EntityName;

            if (listPageOutModel.MetaData.ColumnMetaDatas == null)
                return View("~/Views/Shared/NotFound.cshtml");

            return View("~/Views/Entity/List.cshtml", listPageOutModel);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var detailPageOutModel = CreateModel();

            return View("~/Views/Entity/Detail.cshtml", detailPageOutModel);
        }

        [HttpGet]
        public virtual ActionResult Detail(Guid? id)
        {
            var detailPageOutModel = DetailModel(id);

            if (detailPageOutModel == null)
                return RedirectToAction("List");

            return View("~/Views/Entity/Detail.cshtml", detailPageOutModel);
        }

        #endregion

        [NonAction]
        protected virtual Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

        [NonAction]
        protected virtual object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
        
        [NonAction]
        protected virtual List<ListItem> GetDataSourceByEntityName(string entityName, Guid?[] entityIds = null)
        {
            var result = this.GetType()
                .GetMethod("GetDataSource", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(Type.GetType(string.Format("DynamicView.Entity.{0}, DynamicView", entityName)))
                .Invoke(this, new object[] { entityIds }) as List<ListItem>;

            return result;
        }

        [NonAction]
        protected virtual List<ListItem> GetDataSource<TSource>(Guid?[] entityIds = null)
            where TSource : class, IEntity, new()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                var result = context.Set<TSource>()
                    .Select(p => new
                    {
                        p.Title,
                        p.Id
                    })
                    .ToList()
                    .Select(p => new ListItem()
                    {
                        Text = p.Title,
                        Value = p.Id.ToString(),
                        Selected = entityIds != null && entityIds.Contains(p.Id)
                    })
                    .ToList();

                return result;
            }
        }







        [NonAction]
        protected virtual ListPageOutModel GetListPageOutModel(Type entityType, Guid? sourceId = null, Type sourceType = null, string relatedWith = null)
        {
            var listPageOutModel = new ListPageOutModel()
            {
                EntityType = entityType,
                MetaData = new ListEntityMetaData()
                {
                    EntityName = entityType.Name
                }
            };

            #region Create Metadata

            var uiPropertyType = typeof(UIPropertyAttribute);

            var entityUIProperty = entityType.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

            if (entityUIProperty != null)
            {
                listPageOutModel.MetaData = new ListEntityMetaData()
                {
                    EntityName = entityUIProperty.Title,
                    CreatePageUrl = String.Format("/{0}/Create{1}", entityType.Name, string.IsNullOrEmpty(relatedWith) ? "" : string.Format("?Relation={0}&RelationId={1}", sourceType.Name, sourceId)),
                    DeleteUrl = String.Format("/api/{0}", entityType.Name),
                    DetailPageUrl = String.Format("/{0}/Detail/", entityType.Name),
                    DataUrl = String.Format("/api/{0}", entityType.Name),
                    ColumnMetaDatas = new List<ColumnMetaData>()
                };

                foreach (var property in entityType.GetProperties())
                {
                    var propertyUIProperty = property.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

                    if (propertyUIProperty == null || propertyUIProperty.HideColumn)
                        continue;

                    var columnMetaData = new ColumnMetaData()
                    {
                        Title = propertyUIProperty.Title,
                        Orderable = propertyUIProperty.Orderable,
                        ColumnType = propertyUIProperty.ColumnType == UIColumnType.Date ? "date" : null,
                        Renderer = propertyUIProperty.ColumnType == UIColumnType.Date ? "return moment(data).format(GlobalConfig.ShortDatePattern.toUpperCase() + \" - \" + GlobalConfig.LongTimePattern);" : null,
                        ColumnName = !string.IsNullOrEmpty(propertyUIProperty.ColumnName) ? propertyUIProperty.ColumnName : string.IsNullOrEmpty(propertyUIProperty.PropertyName) ? property.Name : propertyUIProperty.PropertyName,
                        Order = propertyUIProperty.ColumnOrder
                    };

                    var isNullable = ReflectionUtils.IsNullable(property.PropertyType);
                    var propertyType = isNullable
                                    ? Nullable.GetUnderlyingType(property.PropertyType)
                                    : property.PropertyType;

                    if (propertyType == typeof(bool))
                    {
                        columnMetaData.Renderer = "return ";

                        columnMetaData.Renderer += string.Format("data == {0} ? \"{1}\" : ", bool.TrueString.ToLower(),
                            string.Format("<span class='label label-sm label-{0}'>{1}</span>",
                                    "success", "Evet"));

                        columnMetaData.Renderer += string.Format("data == {0} ? \"{1}\" : ", bool.FalseString.ToLower(),
                            string.Format("<span class='label label-sm label-{0}'>{1}</span>",
                                    "danger", "Hayır"));

                        columnMetaData.Renderer += "\"\";";
                    }
                    else if (!string.IsNullOrEmpty(propertyUIProperty.EnumType))
                    {
                        var enumType = GetEnumType(propertyUIProperty.EnumType);

                        if (enumType != null)
                        {
                            columnMetaData.Renderer = "return ";

                            foreach (var value in Enum.GetValues(enumType))
                            {
                                var memberInfo = enumType.GetMember(value.ToString());
                                var enumUIProperty =
                                    memberInfo[0].GetCustomAttributes(typeof(UIPropertyAttribute), false)
                                        .FirstOrDefault() as UIPropertyAttribute;

                                if (enumUIProperty != null)
                                {
                                    columnMetaData.Renderer += string.Format("data == {0} ? \"{1}\" : ", (int)value,
                                        string.IsNullOrEmpty(enumUIProperty.LabelCls)
                                            ? enumUIProperty.Title
                                            : string.Format("<span class='label label-sm label-{0}'>{1}</span>",
                                                enumUIProperty.LabelCls, enumUIProperty.Title));
                                }
                            }

                            columnMetaData.Renderer += "\"\";";

                        }
                    }

                    listPageOutModel.MetaData.ColumnMetaDatas.Add(columnMetaData);
                }

                listPageOutModel.MetaData.ColumnMetaDatas =
                    listPageOutModel.MetaData.ColumnMetaDatas.OrderBy(p => p.Order).ToList();
            }

            #endregion

            return listPageOutModel;
        }

        [NonAction]
        protected virtual DetailPageOutModel CreateModel()
        {
            var entityType = typeof(T);

            var detailPageOutModel = new DetailPageOutModel()
            {
                EntityType = entityType,
                MetaData = new DetailEntityMetaData()
                {
                    IsNew = true,
                    Title = String.Format("{0} Oluştur", entityType.Name)
                }
            };

            #region Create Metadata

            var uiPropertyType = typeof(UIPropertyAttribute);

            var entityUIProperty = entityType.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

            if (entityUIProperty != null)
            {
                detailPageOutModel.MetaData = new DetailEntityMetaData()
                {
                    Title = String.Format("{0} Oluştur", entityUIProperty.SingularizeTitle),
                    IsNew = true,
                    FormMetaDatas = new List<FormMetaData>()
                };

                var entity = new T();

                foreach (var property in entityType.GetProperties())
                {
                    var propertyUIProperty =
                        property.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

                    if (propertyUIProperty == null || propertyUIProperty.HideForm)
                        continue;

                    var formMetaData = new FormMetaData()
                    {
                        IsPrimaryKey = propertyUIProperty.IsPrimaryKey,
                        Title = propertyUIProperty.Title,
                        PropertyName = string.IsNullOrEmpty(propertyUIProperty.PropertyName) ? property.Name : propertyUIProperty.PropertyName,
                        UIFormInputType = propertyUIProperty.FormInputType,
                        IsRequired = propertyUIProperty.IsRequired,
                        MaxLength = propertyUIProperty.MaxLength,
                        Items = propertyUIProperty.Items ?? new List<ListItem>(),
                        PropertyType = property.PropertyType,
                        FormOrder = propertyUIProperty.FormOrder,
                        Mask = propertyUIProperty.Mask
                    };

                    if (formMetaData.UIFormInputType != UIFormInputType.Password
                        && formMetaData.UIFormInputType != UIFormInputType.PasswordConfirm
                        && formMetaData.UIFormInputType != UIFormInputType.PasswordWithConfirm)
                    {
                        if (formMetaData.IsPrimaryKey)
                            formMetaData.Value = IdentityGenerator.NewSequentialGuid();
                        else
                            formMetaData.Value = property.GetValue(entity);
                    }

                    var isNullable = ReflectionUtils.IsNullable(property.PropertyType);
                    var propertyType = isNullable
                         ? Nullable.GetUnderlyingType(property.PropertyType)
                         : property.PropertyType;

                    if (formMetaData.UIFormInputType == UIFormInputType.None)
                    {
                        if (!string.IsNullOrEmpty(propertyUIProperty.EnumType))
                        {
                            formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                        }
                        else
                        {
                            if (propertyType == typeof(string))
                                formMetaData.UIFormInputType = UIFormInputType.Text;
                            else if (propertyType == typeof(int))
                                formMetaData.UIFormInputType = UIFormInputType.IntField;
                            else if (propertyType == typeof(decimal))
                                formMetaData.UIFormInputType = UIFormInputType.DecimalField;
                            else if (propertyType == typeof(double))
                                formMetaData.UIFormInputType = UIFormInputType.DecimalField;
                            else if (propertyType == typeof(bool))
                                formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                            else if (propertyType.IsEnum)
                                formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                        }
                    }

                    if (formMetaData.UIFormInputType == UIFormInputType.Dropdown
                        && (propertyUIProperty.Items == null || !propertyUIProperty.Items.Any()))
                    {
                        if (!string.IsNullOrEmpty(propertyUIProperty.EnumType))
                        {
                            #region Enum Items

                            formMetaData.Items.Add(new ListItem()
                            {
                                Text = "Seçiniz",
                                Value = "",
                                Selected = detailPageOutModel.MetaData.IsNew
                                                ? string.IsNullOrEmpty(propertyUIProperty.SelectedItem)
                                                : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                            });

                            var enumType = GetEnumType(propertyUIProperty.EnumType);

                            foreach (var value in Enum.GetValues(enumType))
                            {
                                var memberInfo = enumType.GetMember(value.ToString());
                                var enumUIProperty = memberInfo[0].GetCustomAttributes(typeof(UIPropertyAttribute), false).FirstOrDefault() as UIPropertyAttribute;

                                if (enumUIProperty != null)
                                {
                                    formMetaData.Items.Add(new ListItem()
                                    {
                                        Text = enumUIProperty.Title,
                                        Value = ((int)value).ToString(),
                                        Selected = detailPageOutModel.MetaData.IsNew
                                                        ? propertyUIProperty.SelectedItem == ((int)value).ToString()
                                                        : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                                    });
                                }
                            }

                            #endregion
                        }
                        else if (propertyType == typeof(bool))
                        {
                            #region Bool Items

                            formMetaData.Items.Add(new ListItem()
                            {
                                Text = "Seçiniz",
                                Value = "",
                                Selected = detailPageOutModel.MetaData.IsNew
                                       ? string.IsNullOrEmpty(propertyUIProperty.SelectedItem)
                                       : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                            });

                            formMetaData.Items.Add(new ListItem()
                            {
                                Text = "Evet",
                                Value = "true",
                                Selected = detailPageOutModel.MetaData.IsNew
                                 ? propertyUIProperty.SelectedItem == "true"
                                 : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                            });

                            formMetaData.Items.Add(new ListItem()
                            {
                                Text = "Hayır",
                                Value = "false",
                                Selected = detailPageOutModel.MetaData.IsNew
                                 ? propertyUIProperty.SelectedItem == "false"
                                 : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                            });

                            #endregion
                        }
                        else if (!string.IsNullOrEmpty(propertyUIProperty.DataSource))
                        {
                            if (Request.Query["Relation"] == propertyUIProperty.DataSource)
                            {
                                formMetaData.UIFormInputType = UIFormInputType.Hidden;
                                formMetaData.Value = Request.Query["RelationId"];
                            }
                            else
                            {
                                #region DataSource From Entity

                                formMetaData.Items.Add(new ListItem()
                                {
                                    Text = "Seçiniz",
                                    Value = "",
                                    Selected = detailPageOutModel.MetaData.IsNew
                                                    ? string.IsNullOrEmpty(propertyUIProperty.SelectedItem)
                                                    : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                                });

                                var result = GetDataSourceByEntityName(propertyUIProperty.DataSource,
                                    new[] { (Guid?)formMetaData.Value });

                                formMetaData.Items.AddRange(result);

                                #endregion
                            }
                        }
                    }

                    if (formMetaData.UIFormInputType == UIFormInputType.ManyToMany
                                    && propertyType.IsGenericType
                                    && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        var baseType = propertyType.GetGenericArguments()[0];

                        #region DataSource From Entity

                        var result = GetDataSourceByEntityName(baseType.Name);

                        formMetaData.Items.AddRange(result);

                        #endregion
                    }

                    if (formMetaData.UIFormInputType == UIFormInputType.PasswordWithConfirm)
                    {
                        formMetaData.UIFormInputType = UIFormInputType.Password;
                        formMetaData.IsRequired = true;
                        detailPageOutModel.MetaData.FormMetaDatas.Add(formMetaData);

                        var confirmMetaData = new FormMetaData()
                        {
                            IsPrimaryKey = formMetaData.IsPrimaryKey,
                            Title = formMetaData.Title + " (Tekrar)",
                            PropertyName = formMetaData.PropertyName + "_Confirm",
                            UIFormInputType = UIFormInputType.PasswordConfirm,
                            IsRequired = true,
                            MaxLength = formMetaData.MaxLength,
                            Items = formMetaData.Items,
                            PropertyType = formMetaData.PropertyType,
                            FormOrder = formMetaData.FormOrder + 1
                        };

                        detailPageOutModel.MetaData.FormMetaDatas.Add(confirmMetaData);
                    }
                    else
                        detailPageOutModel.MetaData.FormMetaDatas.Add(formMetaData);

                }

                detailPageOutModel.MetaData.FormMetaDatas = detailPageOutModel.MetaData.FormMetaDatas
                           .OrderBy(p => p.FormOrder)
                           .ToList();
            }

            #endregion

            ViewBag.Title = detailPageOutModel.MetaData.Title;

            return detailPageOutModel;
        }


        [NonAction]
        protected virtual DetailPageOutModel DetailModel(Guid? id)
        {
            using (var context = new ApplicationContext())
            {
                if (id.HasValue && id.Value != Guid.Empty)
                {
                    var query = context.Set<T>().AsQueryable();

                    if (query.Any(p => p.Id == id))
                    {
                        var entityType = typeof(T);

                        var detailPageOutModel = new DetailPageOutModel()
                        {
                            EntityType = entityType,
                            MetaData = new DetailEntityMetaData()
                            {
                                IsNew = false,
                                Title = String.Format("{0} Düzenle", entityType.Name)
                            }
                        };

                        #region Create Metadata & Relations

                        var uiPropertyType = typeof(UIPropertyAttribute);

                        var entityUIProperty = entityType.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

                        if (entityUIProperty != null)
                        {
                            detailPageOutModel.MetaData = new DetailEntityMetaData()
                            {
                                Title = String.Format("{0} Düzenle", entityUIProperty.SingularizeTitle),
                                IsNew = false,
                                FormMetaDatas = new List<FormMetaData>()
                            };

                            #region Include Many To Many Relations

                            query = entityType.GetProperties()
                                .Where(p =>
                                {
                                    var uiPropertyAttribute =
                                        p.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as
                                            UIPropertyAttribute;
                                    return uiPropertyAttribute != null &&
                                           uiPropertyAttribute.FormInputType == UIFormInputType.ManyToMany;
                                })
                                .Aggregate(query, (current, manytoManyItem) => current.Include(manytoManyItem.Name));

                            #endregion

                            var item = query.FirstOrDefault(p => p.Id == id);

                            foreach (var property in entityType.GetProperties())
                            {
                                var propertyUIProperty =
                                    property.GetCustomAttributes(uiPropertyType, true).FirstOrDefault() as UIPropertyAttribute;

                                if (propertyUIProperty == null || propertyUIProperty.HideForm)
                                    continue;

                                var formMetaData = new FormMetaData()
                                {
                                    IsPrimaryKey = propertyUIProperty.IsPrimaryKey,
                                    Title = propertyUIProperty.Title,
                                    PropertyName = string.IsNullOrEmpty(propertyUIProperty.PropertyName) ? property.Name : propertyUIProperty.PropertyName,
                                    UIFormInputType = propertyUIProperty.FormInputType,
                                    IsRequired = propertyUIProperty.IsRequired,
                                    MaxLength = propertyUIProperty.MaxLength,
                                    Items = propertyUIProperty.Items ?? new List<ListItem>(),
                                    PropertyType = property.PropertyType,
                                    FormOrder = propertyUIProperty.FormOrder,
                                    Mask = propertyUIProperty.Mask
                                };

                                if (formMetaData.UIFormInputType != UIFormInputType.List
                                    && formMetaData.UIFormInputType != UIFormInputType.Password
                                    && formMetaData.UIFormInputType != UIFormInputType.PasswordConfirm
                                    && formMetaData.UIFormInputType != UIFormInputType.PasswordWithConfirm)
                                {
                                    formMetaData.Value = property.GetValue(item);
                                }

                                var isNullable = ReflectionUtils.IsNullable(property.PropertyType);
                                var propertyType = isNullable
                                     ? Nullable.GetUnderlyingType(property.PropertyType)
                                     : property.PropertyType;

                                if (formMetaData.UIFormInputType == UIFormInputType.None)
                                {
                                    if (!string.IsNullOrEmpty(propertyUIProperty.EnumType))
                                    {
                                        formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                                    }
                                    else
                                    {
                                        if (propertyType == typeof(string))
                                            formMetaData.UIFormInputType = UIFormInputType.Text;
                                        else if (propertyType == typeof(int))
                                            formMetaData.UIFormInputType = UIFormInputType.IntField;
                                        else if (propertyType == typeof(decimal))
                                            formMetaData.UIFormInputType = UIFormInputType.DecimalField;
                                        else if (propertyType == typeof(double))
                                            formMetaData.UIFormInputType = UIFormInputType.DecimalField;
                                        else if (propertyType == typeof(bool))
                                            formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                                        else if (propertyType.IsEnum)
                                            formMetaData.UIFormInputType = UIFormInputType.Dropdown;
                                        else if (propertyType == typeof(DateTime))
                                            formMetaData.UIFormInputType = UIFormInputType.Date;
                                    }
                                }

                                if (formMetaData.UIFormInputType == UIFormInputType.Dropdown
                                    && (propertyUIProperty.Items == null || !propertyUIProperty.Items.Any()))
                                {
                                    if (!string.IsNullOrEmpty(propertyUIProperty.EnumType))
                                    {
                                        formMetaData.Items.Add(new ListItem()
                                        {
                                            Text = "Seçiniz",
                                            Value = "",
                                            Selected = formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                                        });

                                        var enumType = GetEnumType(propertyUIProperty.EnumType);

                                        foreach (var value in Enum.GetValues(enumType))
                                        {
                                            var memberInfo = enumType.GetMember(value.ToString());
                                            var enumUIProperty = memberInfo[0].GetCustomAttributes(typeof(UIPropertyAttribute), false).FirstOrDefault() as UIPropertyAttribute;

                                            if (enumUIProperty != null)
                                            {
                                                formMetaData.Items.Add(new ListItem()
                                                {
                                                    Text = enumUIProperty.Title,
                                                    Value = ((int)value).ToString(),
                                                    Selected = (int?)formMetaData.Value == (int)value
                                                });
                                            }
                                        }
                                    }
                                    else if (propertyType == typeof(bool))
                                    {
                                        formMetaData.Items.Add(new ListItem()
                                        {
                                            Text = "Seçiniz",
                                            Value = "",
                                            Selected = formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                                        });

                                        formMetaData.Items.Add(new ListItem()
                                        {
                                            Text = "Evet",
                                            Value = "true",
                                            Selected = (bool?)formMetaData.Value == true
                                        });

                                        formMetaData.Items.Add(new ListItem()
                                        {
                                            Text = "Hayır",
                                            Value = "false",
                                            Selected = (bool?)formMetaData.Value == false
                                        });
                                    }
                                    else if (!string.IsNullOrEmpty(propertyUIProperty.DataSource))
                                    {
                                        if (Request.Query["Relation"] == propertyUIProperty.DataSource)
                                        {
                                            formMetaData.UIFormInputType = UIFormInputType.Hidden;
                                            formMetaData.Value = Request.Query["RelationId"];
                                        }
                                        else
                                        {
                                            #region DataSource From Entity

                                            formMetaData.Items.Add(new ListItem()
                                            {
                                                Text = "Seçiniz",
                                                Value = "",
                                                Selected = detailPageOutModel.MetaData.IsNew
                                                                ? string.IsNullOrEmpty(propertyUIProperty.SelectedItem)
                                                                : formMetaData.Value == GetDefaultValue(formMetaData.PropertyType)
                                            });

                                            var result = GetDataSourceByEntityName(propertyUIProperty.DataSource, new[] { (Guid?)formMetaData.Value });

                                            formMetaData.Items.AddRange(result);

                                            #endregion
                                        }
                                    }
                                }

                                if (formMetaData.UIFormInputType == UIFormInputType.List
                                    && propertyType.IsGenericType
                                    && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                                {
                                    var baseType = propertyType.GetGenericArguments()[0];

                                    formMetaData.ListPageOutModel = GetListPageOutModel(baseType, id, typeof(T), propertyUIProperty.RelatedWith);
                                    formMetaData.ListPageOutModel.MetaData.RelatedWith = propertyUIProperty.RelatedWith;
                                    formMetaData.ListPageOutModel.MetaData.RelationName = typeof(T).Name;
                                }

                                if (formMetaData.UIFormInputType == UIFormInputType.ManyToMany
                                    && propertyType.IsGenericType
                                    && propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                                {
                                    var baseType = propertyType.GetGenericArguments()[0];

                                    #region DataSource From Entity

                                    var selectedItems = new List<Guid?>();

                                    if (!detailPageOutModel.MetaData.IsNew)
                                    {
                                        var relationItems = formMetaData.Value as IEnumerable;
                                        foreach (var relationItem in relationItems)
                                        {
                                            var relationEntity = relationItem as IEntity;

                                            if (relationEntity != null)
                                            {
                                                selectedItems.Add(relationEntity.Id);
                                            }
                                        }
                                    }

                                    var result = GetDataSourceByEntityName(baseType.Name, selectedItems.ToArray());

                                    formMetaData.Items.AddRange(result);

                                    #endregion
                                }

                                if (formMetaData.UIFormInputType == UIFormInputType.PasswordWithConfirm)
                                {
                                    formMetaData.UIFormInputType = UIFormInputType.Password;
                                    detailPageOutModel.MetaData.FormMetaDatas.Add(formMetaData);

                                    var confirmMetaData = new FormMetaData()
                                    {
                                        IsPrimaryKey = formMetaData.IsPrimaryKey,
                                        Title = formMetaData.Title + " (Tekrar)",
                                        PropertyName = formMetaData.PropertyName + "_Confirm",
                                        UIFormInputType = UIFormInputType.PasswordConfirm,
                                        IsRequired = formMetaData.IsRequired,
                                        MaxLength = formMetaData.MaxLength,
                                        Items = formMetaData.Items,
                                        PropertyType = formMetaData.PropertyType,
                                        FormOrder = formMetaData.FormOrder + 1
                                    };

                                    detailPageOutModel.MetaData.FormMetaDatas.Add(confirmMetaData);
                                }
                                else
                                    detailPageOutModel.MetaData.FormMetaDatas.Add(formMetaData);

                            }

                            detailPageOutModel.MetaData.FormMetaDatas = detailPageOutModel.MetaData.FormMetaDatas
                                .OrderBy(p => p.FormOrder)
                                .ToList();
                        }

                        #endregion

                        string referer = Request.Headers["Referer"].ToString();
                        Uri baseUri = new Uri(referer);

                        ViewBag.IsSuccess = referer != null && baseUri.AbsolutePath.Contains(string.Format("/{0}/Create", typeof(T).Name));
                        ViewBag.Title = detailPageOutModel.MetaData.Title;

                        return detailPageOutModel;
                    }
                }

                return null;
            }
        }
    }
}