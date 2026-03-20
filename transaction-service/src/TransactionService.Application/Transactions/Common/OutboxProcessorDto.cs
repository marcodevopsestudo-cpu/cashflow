using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransactionService.Application.Transactions.Common
{
    public sealed record OutboxProcessorDto(
        int processedItems
    );
}
    
