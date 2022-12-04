using MEPNumerator.Model.Entities;
using MEPNumerator.Data.Repositories;
using MEPNumerator.DataAccess;

namespace MEPNumerator.Data.Repositories
{
    public class MechanicRepository : GenericRepository<Mechanic, MEPNumeratorDbContext>, IMechanicRepository
    {
        public MechanicRepository(MEPNumeratorDbContext context) : base(context)
        {
        }
    }

}
