namespace MiniORM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    public class DbSet<TEntity> : ICollection<TEntity> where TEntity : class, new()
    {
        internal DbSet(IEnumerable<TEntity> entities) {
            this.Entities = entities.ToList();
            this.ChangeTracker = new ChangeTracker<TEntity>(this.Entities);
        }

        internal ChangeTracker<TEntity> ChangeTracker { get; set; }

        internal IList<TEntity> Entities { get; set; }

        public int Count => this.Entities.Count;

        public bool IsReadOnly => this.Entities.IsReadOnly;

        public void Add(TEntity entity) {
            this.Entities.Add(entity);
            this.ChangeTracker.Add(entity);
        }
        public void Clear() {
            while (this.Entities.Any())
            {
                var entity = this.Entities.First();
                this.Remove(entity);
            }
        }
        public bool Contains(TEntity item) => this.Entities.Contains(item);

        public void CopyTo(TEntity[] array, int arrayIndex) {
            this.Entities.CopyTo(array, arrayIndex);
        }
        public bool Remove(TEntity item) {
            bool removed = this.Entities.Remove(item);
            if (removed)
            {
                this.ChangeTracker.Remove(item);
            }
            return removed;
        }

        public void RemoveRange(IEnumerable<TEntity> entities) {
            foreach (var e in entities.ToList())
            {
                this.Remove(e);
            }
        }

        public IEnumerator<TEntity> GetEnumerator() {
            return this.Entities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}
