using DynamicView.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Entity
{
    [Table("Phone")]
    [UIProperty(Title = "Telefon Numaraları", SingularizeTitle = "Telefon Numarası")]
    public partial class Phone : BaseEntity
    {
        [Required]
        [StringLength(255)]
        [UIProperty(Title = "Telefon Numarası", ColumnOrder = 1, FormOrder = 1, MaxLength = 255, IsRequired = true, Mask = "(999) 999-9999")]
        public override string Title { get; set; }

        [Required]
        [UIProperty(Title = "Telefon Türü", ColumnOrder = 4, FormOrder = 4, EnumType = "Domain.Enums.PhoneType", IsRequired = true, SelectedItem = "1")]
        public int PhoneType { get; set; }
    }
}
