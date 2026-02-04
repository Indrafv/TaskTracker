using System;
using System.Collections.Generic;
using System.Text;
using TaskTracker.Models;

namespace TaskTracker.Interfaces
{
    internal interface ITaskService
    {
        Task<List<AppTask>> GetAllTasks();
        Task<int> AddNewTask(string description);
        Task<bool> UpdateTask(int id, string description);
        Task<bool> DeleteTask(int taskId);
        Task<bool> SetStatus(string status, int id);
        Task<List<AppTask>> GetTasksByStatus(string status);
        List<string> GetAllHelpCommands();
    }
}
