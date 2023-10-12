using test0000001.Models;

namespace test0000001.Repository.InterfaceClass
{
    public interface IDuration
    {
        Task<IEnumerable<Duration>> GetAllDuration();
        Task<Duration> GetDurationById(int id);
        Task<bool> addDuration(Duration newDuration);
        Task<bool> editDuration(Duration editDuration);
        Task<bool> deleteDuration(int id);

    }
}
