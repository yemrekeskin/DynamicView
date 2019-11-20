using DynamicView.Attributes;
using DynamicView.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Models
{
    public class DetailPageOutModel
    {
        public Type EntityType { get; set; }
        public DetailEntityMetaData MetaData { get; set; }

    }

    public class DetailEntityMetaData
    {
        public bool IsNew { get; set; }
        public string Title { get; set; }

        public List<FormMetaData> FormMetaDatas { get; set; }
    }

    public class FormMetaData
    {
        public bool IsPrimaryKey { get; set; }
        public string Title { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public object Value { get; set; }
        public UIFormInputType UIFormInputType { get; set; }
        public bool IsRequired { get; set; }
        public int MaxLength { get; set; }
        public List<ListItem> Items { get; set; }
        public int FormOrder { get; set; }
        public string Mask { get; set; }

        public ListPageOutModel ListPageOutModel { get; set; }

        public T GetValue<T>()
        {
            if (IsNullable() && Value == null)
                return default(T);

            return (T)Value;
        }

        public bool IsNullable()
        {
            return ReflectionUtils.IsNullable(PropertyType);
        }

        public Type BasePropertyType()
        {
            return IsNullable()
                ? Nullable.GetUnderlyingType(PropertyType)
                : PropertyType;
        }
    }
}
