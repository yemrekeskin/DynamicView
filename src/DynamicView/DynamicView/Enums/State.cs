using DynamicView.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Enums
{
    public enum State
    {
        [UIProperty(Title = "Aktif", LabelCls = "success")]
        Online = 1,

        [UIProperty(Title = "Pasif", LabelCls = "warning")]
        Offline = 2,

        Delete = 4
    }
}
