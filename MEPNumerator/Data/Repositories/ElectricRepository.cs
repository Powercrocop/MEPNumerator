using MEPNumerator.DataAccess;
using MEPNumerator.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEPNumerator.Data.Repositories
{
    public class ElectricRepository : GenericRepository<Electric, MEPNumeratorDbContext>, IElectricRepository
    {
        public ElectricRepository(MEPNumeratorDbContext context) : base(context)
        {
        }
    }
}
