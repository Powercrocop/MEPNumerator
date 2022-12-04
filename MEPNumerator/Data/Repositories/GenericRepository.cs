using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPNumerator.Data.Repositories
{
    public class GenericRepository<TEntity,TContext> : IGenericRepository<TEntity>
        where TEntity:class
        where TContext:DbContext
    {
        protected readonly TContext Context;
        protected GenericRepository(TContext context)
        {
            this.Context = context;
        }
        public TContext IsDatabaseChanged()
        {
            return Context;
            //return Context.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
            
        }
        public void Add(TEntity model)
        {
            Context.Set<TEntity>().Add(model);
        }
        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }
        public bool HasChanges()
        {
            return Context.ChangeTracker.HasChanges();
        }
        public void Remove(TEntity model)
        {
            Context.Set<TEntity>().Remove(model);
        }
        public async Task SaveAsync()
        {
            await Context.SaveChangesAsync();
        }
        public List<TEntity> GetAll()
        {
            List<TEntity> list = new List<TEntity>();
            var tEntity = Context.Set<TEntity>();
            foreach (var model in tEntity)
            {
                list.Add(model);
            }
            return list;
        }

    }
}
