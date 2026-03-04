using Qtemplate.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Domain.Interfaces.Repositories
{
    public interface IAnalyticsRepository
    {
        Task AddAsync(Analytics analytics);
        Task UpdateTimeOnPageAsync(string sessionId, string pageUrl, int seconds);
        Task<List<Analytics>> GetByDateRangeAsync(DateTime from, DateTime to);
    }
}
