using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Models
{
    public class ListPageOutModel
    {
        public Type EntityType { get; set; }
        public ListEntityMetaData MetaData { get; set; }
    }

    public class ListEntityMetaData
    {
        public string EntityName { get; set; }
        public string DataUrl { get; set; }
        public string CreatePageUrl { get; set; }
        public string DetailPageUrl { get; set; }
        public string DeleteUrl { get; set; }
        public string RelatedWith { get; set; }
        public string RelationName { get; set; }

        public List<ColumnMetaData> ColumnMetaDatas { get; set; }
    }

    public class ColumnMetaData
    {
        public string Title { get; set; }
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string Renderer { get; set; }
        public bool Orderable { get; set; }
        public int Order { get; set; }
    }
}
