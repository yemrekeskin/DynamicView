using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Attributes
{
    public class UIPropertyAttribute 
        : Attribute
    {
        #region Constructor

        public UIPropertyAttribute()
        {
            Orderable = true;
            ColumnType = UIColumnType.Text;
            FormInputType = UIFormInputType.None;
        }

        #endregion

        #region Shared

        public bool IsPrimaryKey { get; set; }
        public string Title { get; set; }
        public string SingularizeTitle { get; set; }
        public string EnumType { get; set; }
        public string PropertyName { get; set; }

        #endregion

        #region Table

        public string ColumnName { get; set; }
        public int ColumnOrder { get; set; }
        public UIColumnType ColumnType { get; set; }
        public bool HideColumn { get; set; }
        public string Renderer { get; set; }
        public bool Orderable { get; set; }
        public string LabelCls { get; set; }

        #endregion

        #region Form

        public int FormOrder { get; set; }
        public UIFormInputType FormInputType { get; set; }
        public bool HideForm { get; set; }
        public bool IsRequired { get; set; }
        public int MaxLength { get; set; }
        public string SelectedItem { get; set; }
        public List<ListItem> Items { get; set; }
        public string DataSource { get; set; }
        public string RelatedWith { get; set; }
        public string Mask { get; set; }

        #endregion
    }

    public enum UIColumnType
    {
        Text = 1,
        Date = 2
    }

    public enum UIFormInputType
    {
        None = 0,
        Text = 1,
        ImageUpload = 2,
        TextArea = 3,
        FileUpload = 4,
        IntField = 5,
        DecimalField = 6,
        Dropdown = 7,
        Date = 8,
        CheckBox = 9,
        Hidden = 10,
        List = 11,
        ManyToMany = 12,
        Password = 13,
        PasswordConfirm = 14,
        PasswordWithConfirm = 15,
        DateTime = 16,
        WYSIHtml5Editor = 17
    }

    public class ListItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
