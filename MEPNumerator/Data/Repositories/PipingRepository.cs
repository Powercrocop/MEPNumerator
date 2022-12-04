using MEPNumerator.DataAccess;
using MEPNumerator.Model.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEPNumerator.Data.Repositories
{
    public class PipingRepository : GenericRepository<Piping, MEPNumeratorDbContext>, IPipingRepository
    {
        public PipingRepository(MEPNumeratorDbContext context) : base(context)
        {
        }
    }
}
