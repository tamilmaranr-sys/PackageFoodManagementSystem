
using PackageFoodManagementSystem.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PackageFoodManagementSystem.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<Inventory>> GetInventoryListAsync();
        //Task<byte[]> GenerateReportAsync(int month, int year, string category = null);
        Task AddBatchToInventoryAsync(Batch batch);
    }
}
