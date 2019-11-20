using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Entity
{
    public interface IEntity
    {
        Guid Id { get; set; }

        string Title { get; set; }

        int State { get; set; }

        DateTime? CreatedAt { get; set; }

        DateTime? ModifiedAt { get; set; }

        Guid? CreatedById { get; set; }

        Guid? ModifiedById { get; set; }
    }
}
