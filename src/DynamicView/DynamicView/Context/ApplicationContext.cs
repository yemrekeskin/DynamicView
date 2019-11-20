using DynamicView.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicView.Context
{
    public partial class ApplicationContext 
        : DbContext
    {
        public virtual DbSet<Phone> Phone { get; set; }
        public virtual DbSet<Contract> Contract { get; set; }

        public ApplicationContext(DbContextOptions options) 
            : base(options)
        {
        }

        public ApplicationContext()
        {

        }
    }
}
