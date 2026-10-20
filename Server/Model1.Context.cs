
//------------------------------------------------------------------------------

namespace Server
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class AutosalonNewEntities1 : DbContext
    {
        public AutosalonNewEntities1()
            : base("name=AutosalonNewEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Cars> Cars { get; set; }
        public virtual DbSet<SoldCars> SoldCars { get; set; }
    }
}
