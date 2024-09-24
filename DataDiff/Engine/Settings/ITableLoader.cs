using DataDiff.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataDiff.Engine.Settings
{
    public interface ITableLoader
    {
        public List<SystemDatabaseTable> Tables { get; }
        Task<OperationResult> AddTable(string name, string description, string dependentTableName);
        Task<OperationResult> AddColumnToTable(string tableName, SystemDatabaseTableColumn column);
    }
}
