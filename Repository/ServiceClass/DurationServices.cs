using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{
    public class DurationServices : IDuration
    {
        private DatabaseContext db;

        public DurationServices(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<bool> addDuration(Duration newDuration)
        {
            await db.Duration!.AddAsync(newDuration);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> editDuration(Duration editDuration)
        {
            var duration = await db.Duration!.SingleOrDefaultAsync(u => u.Id!.Equals(editDuration.Id));
            if (duration != null)
            {
                duration.PriceAmount = editDuration.PriceAmount;
                duration.Term = editDuration.Term;
                await db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<IEnumerable<Duration>> GetAllDuration()
        {
            return await db.Duration!.ToListAsync();
        }

        public async Task<bool> deleteDuration(int id)
        {
            var duration = await db.Duration!.SingleOrDefaultAsync(t => t.Id.Equals(id));
            if (duration == null)
            {
                return false;
            }
            db.Duration!.Remove(duration);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<Duration> GetDurationById(int id)
        {
            return await db.Duration!.FirstOrDefaultAsync(c => c.Id == id);
        }

    }
}
