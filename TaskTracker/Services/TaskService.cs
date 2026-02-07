using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
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
                CreateFileIfNotExists();
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

        public async Task<bool> DeleteTask(int taskId)
        {

            if (!File.Exists(FilePath))
            {
                return Task.FromResult(false).Result;
            }

            try
            {
                var json = await File.ReadAllTextAsync(FilePath).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                var tasks = JsonSerializer.Deserialize<List<AppTask>>(json) ?? new List<AppTask>();
                var index = tasks.FindIndex(x => x.id == taskId);
                if (index < 0)
                {
                    return false;
                }

                tasks.RemoveAt(index);
                var updatedJson = JsonSerializer.Serialize<List<AppTask>>(tasks);
                await File.WriteAllTextAsync(FilePath, updatedJson).ConfigureAwait(false);
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO error deleting task {taskId}. Error - " + ex.Message);
                return false;
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.WriteLine($"JSON error deleting task {taskId}. Error - " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Task deletion failed. Error - " + ex.Message);
                return false;
            }
        }


        public List<string> GetAllHelpCommands()
        {
            return new List<string>
            {
                "add \"Task Description\" - To add a new task, type add with task description",
                "update \"Task Id\" \"Task Description\" - To update a task, type update with task id and task description",
                "delete \"Task Id\" - To delete a task, type delete with task id",
                "mark-in-progress \"Task Id\" - To mark a task to in progress, type mark-in-progress with task id",
                "mark-done \"Task Id\" - To mark a task to done, type mark-done with task id",
                "list - To list all task with its current status",
                "list done - To list all task with done status",
                "list todo  - To list all task with todo status",
                "list in-progress  - To list all task with in-progress status",
                "exit - To exit from app",
                "clear - To clear console window"
            };
        }

        private static void UpdateJsonFile(Task<List<AppTask>> tasksFromJson)
        {
            string updatedAppTasks = JsonSerializer.Serialize<List<AppTask>>(tasksFromJson.Result);
            File.WriteAllText(FilePath, updatedAppTasks);
        }

        public static Task<List<AppTask>> GetTaskFromJson()
        {
            string tasksFromJsonFileString = File.ReadAllText(FilePath);
            if (tasksFromJsonFileString == null)
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
                if (!File.Exists(FilePath))
                {
                    return Task.FromResult(new List<Models.AppTask>());
                }

                string jsonString = File.ReadAllText(FilePath);

                if (!string.IsNullOrEmpty(jsonString))
                {
                    var tasks = JsonSerializer.Deserialize<List<Models.AppTask>>(jsonString);
                    var statusCheck = GetStatusEnum(status);
                    return  Task.FromResult(tasks?.Where(x => x.status == statusCheck).ToList() ?? []);
                }
                else
                {
                    return Task.FromResult(new List<Models.AppTask>());
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> SetStatus(string status, int id)
        {
            if(!File.Exists(FilePath))
            {
                return Task.FromResult(false);
            }

            string jsonString = File.ReadAllText(FilePath);
            if (!string.IsNullOrEmpty(jsonString)) {
                var tasks = JsonSerializer.Deserialize<List<Models.AppTask>>(jsonString);
                var taskToBeUpdated = tasks?.Where(x => x.id == id).SingleOrDefault();
                if (taskToBeUpdated != null)
                {
                    taskToBeUpdated.status = GetStatusEnum(status);
                    taskToBeUpdated.updatedAt = DateTime.Now;
                    UpdateJsonFile(Task.FromResult(tasks ?? []));
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }

        public Task<bool> UpdateTask(int id, string description)
        {
            if (!File.Exists(FilePath))
            {
                return Task.FromResult(false);
            }
               
            string jsonString = File.ReadAllText(FilePath);
            if (!string.IsNullOrEmpty(jsonString))
            {
                var tasks = JsonSerializer.Deserialize<List<Models.AppTask>>(jsonString);
                var taskToBeUpdated = tasks?.Where(x => x.id == id).SingleOrDefault();
                if (taskToBeUpdated != null)
                {
                    taskToBeUpdated.description = description;
                    taskToBeUpdated.updatedAt = DateTime.Now;
                    UpdateJsonFile(Task.FromResult(tasks ?? []));
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
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

        private StatusEnum GetStatusEnum(string status)
        {
            switch (status)
            {
                case "mark-in-progress":
                    return StatusEnum.in_progress;
                case "mark-done":
                    return StatusEnum.done;
                case "mark-todo":
                    return StatusEnum.todo;
                default:
                    return StatusEnum.todo;
            }
        }

        
    }
}
