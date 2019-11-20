using DynamicView.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Entity
{
    [UIProperty(Title = "Sözlesmeler", SingularizeTitle = "Sözlesme")]
    public partial class Contract 
        : BaseEntity
    {
        [Required]
        [StringLength(500)]
        [UIProperty(Title = "Adı", ColumnOrder = 1, FormOrder = 1, MaxLength = 500, IsRequired = true)]
        public override string Title { get; set; }

        [UIProperty(Title = "Güncelleme Metni", FormOrder = 4, HideColumn = true/*, MaxLength = 2000*/, FormInputType = UIFormInputType.TextArea)]
        public string Description { get; set; }

        [UIProperty(Title = "Sözlesme Metni", FormOrder = 4, HideColumn = true/*, MaxLength = 2000*/, FormInputType = UIFormInputType.WYSIHtml5Editor)]
        public string Content { get; set; }

        [Required]
        [UIProperty(Title = "Sözlesme Türü", ColumnOrder = 5, FormOrder = 5, EnumType = "Domain.Enums.ContractType", IsRequired = true, SelectedItem = "1")]
        public int ContractType { get; set; }

        [UIProperty(Title = "Geçerlilik Tarihi", ColumnOrder = 6, FormOrder = 6, ColumnType = UIColumnType.Date, FormInputType = UIFormInputType.Date)]
        public DateTime? ExpiredDate { get; set; }                
    }
}
