using System.Numerics;
using test0000001.Models;

namespace test0000001.Repository.InterfaceClass
{
    public interface IPolicy
    {
        Task<IEnumerable<Policy>> GetAllPolicy();
        Task<IEnumerable<Policy>> GetPolicyByCategory(int categoryId);      
        Task<Policy> GetPolicyById(int id);
        Task<bool> AddPolicy(Policy newPolicy);
        Task<bool> EditPolicy(Policy editPolicy);
        Task<bool> DeletePolicy(int id);
    }
}
