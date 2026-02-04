
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using TaskTracker.Enums;
using TaskTracker.Interfaces;
using TaskTracker.Models;

namespace TaskTracker.Services
{
    internal class TaskService : ITaskService
    {

        private static string FileName = "taskJson.json";

        private static string FilePath = Path.Combine(Directory.GetCurrentDirectory(), FileName);

        public Task<int> AddNewTask(string description)
        {

            try
            {
                var appTasks = new List<AppTask>();
                var task = new AppTask
                {
                    id = GetTaskId(),
                    description = description,
                    status = StatusEnum.todo,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now


                };

                var FileCreatedSuccesfully = CreateFileIfNotExists();

                if (FileCreatedSuccesfully)
                {
                    string tasksFromJsonFileString = File.ReadAllText(FilePath);
                    if (!string.IsNullOrEmpty(tasksFromJsonFileString))
                    {
                        appTasks = JsonSerializer.Deserialize<List<AppTask>>(tasksFromJsonFileString);
                    }

                    appTasks?.Add(task);
                    string updateAppTaks = JsonSerializer.Serialize<List<AppTask>>(appTasks ?? new List<AppTask>());
                    File.WriteAllText(FilePath, updateAppTaks);
                    return Task.FromResult(task.id);
                }

                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task addition failed. Error - " + ex.Message);
                return Task.FromResult(0);
            }
            
        }

        private int GetTaskId()
        {
            if (!File.Exists(FilePath))
            {
                return 1;
            }

            else
            {
                string tasksFromJsonFileString = File.ReadAllText(FilePath);
                if (!string.IsNullOrEmpty(tasksFromJsonFileString))
                {
                    var appTasks = JsonSerializer.Deserialize<List<AppTask>>(tasksFromJsonFileString);
                    if (appTasks != null && appTasks.Count > 0)
                    {
                        return appTasks.OrderBy(x => x.id).Last().id + 1;
                    }
                }
            }

            return 1;
        }

        public Task<bool> DeleteTask(int taskId)
        {

            if (!File.Exists(FilePath))
            {
                return Task.FromResult(false);
            }
            try
            {
                var TaskFromJson = GetTaskFromJson();

                if(TaskFromJson.Result.Count > 0)
                {
                    var taskToBeDeleted = TaskFromJson.Result
                    .Where(x => x.id == taskId)
                    .SingleOrDefault();

                    if(taskToBeDeleted != null)
                    {
                        TaskFromJson.Result.Remove(taskToBeDeleted);
                        UpdateJsonFile(TaskFromJson);
                        return Task.FromResult(true);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            throw new NotImplementedException();
        }

        public List<string> GetAllHelpCommands()
        {
            throw new NotImplementedException();
        }

        private static void UpdateJsonFile(Task<List<AppTask>> tasksFromJson)
        {
            string updatedAppTasks = JsonSerializer.Serialize<List<AppTask>>(tasksFromJson.Result);
            File.WriteAllText(FilePath, updatedAppTasks);
        }

        public static Task<List<AppTask>> GetTaskFromJson()
        {
            string tasksFromJsonFileString = File.ReadAllText(FilePath);
            if(tasksFromJsonFileString == null)
            {
                return Task.FromResult(JsonSerializer.Deserialize<List<AppTask>>(tasksFromJsonFileString) ?? []);
            }

            return Task.FromResult(new List<AppTask>());
        }

        public Task<List<AppTask>> GetAllTasks()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return System.Threading.Tasks.Task.FromResult(new List<Models.AppTask>());
                }

                string jsonString = File.ReadAllText(FilePath);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    List<Models.AppTask> tasks = JsonSerializer.Deserialize<List<Models.AppTask>>(jsonString);
                    return System.Threading.Tasks.Task.FromResult(tasks ?? []);
                }

                else
                {
                    return System.Threading.Tasks.Task.FromResult(new List<Models.AppTask>());
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public Task<List<AppTask>> GetTasksByStatus(string status)
        {
            try
            {
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> SetStatus(string status, int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateTask(int id, string description)
        {
            throw new NotImplementedException();
        }

        private bool CreateFileIfNotExists()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    using (FileStream fs = File.Create(FileName))
                    {
                        Console.WriteLine($"File {FileName} Created Succesfully");
                    }
                    
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error, File {FileName} has failed: {ex.Message}");
                return false;
            }
        }
    }
}
