using DynamicView.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Entity
{
    public class BaseEntity : IEntity
    {
        [UIProperty(IsPrimaryKey = true, FormInputType = UIFormInputType.Hidden)]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        [UIProperty(Title = "Başlık", ColumnOrder = 1, FormOrder = 1, MaxLength = 255, IsRequired = true)]
        public virtual string Title { get; set; }

        [UIProperty(Title = "Durum", ColumnOrder = 2, FormOrder = 2, EnumType = "DynamicView.Enums.State", IsRequired = true, SelectedItem = "1")]
        public int State { get; set; }

        [UIProperty(Title = "Oluşturma Tarihi", ColumnOrder = 4, ColumnType = UIColumnType.Date, HideForm = true)]
        public DateTime? CreatedAt { get; set; }

        [UIProperty(HideColumn = true, HideForm = true)]
        public DateTime? ModifiedAt { get; set; }

        [UIProperty(HideColumn = true, HideForm = true)]
        public Guid? CreatedById { get; set; }

        [UIProperty(HideColumn = true, HideForm = true)]
        public Guid? ModifiedById { get; set; }
    }
}
