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

                // Instanciare a new list of AppTask to hold the existing tasks from json file and the new task to be added
                var appTasks = new List<AppTask>();
                var task = new AppTask
                {
                    id = GetTaskId(),
                    description = description,
                    status = StatusEnum.todo,
                    createdAt = DateTime.Now,
                    updatedAt = DateTime.Now


                };
                // Check if the json file exists, if not create a new one and add the new task to it, if it exists read the existing tasks from it and add the new task to the list and update the json file with the new list of tasks
                var FileCreatedSuccesfully = CreateFileIfNotExists();

                // If the file is created successfully or already exists, read the existing tasks from it and add the new task to the list and update the json file with the new list of tasks
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
                // If the file is not created successfully, return 0 as the task id to indicate that the task addition failed
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

                // Read the existing tasks from the json file and return the next task id by getting the max id from the existing tasks and adding 1 to it, if there are no existing tasks return 1 as the task id for the new task
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

                // Read the existing tasks from the json file, find the task with the given task id and remove it from the list of tasks, if the task is not found return false to indicate that the task deletion failed, if the task is found and removed from the list update the json file with the new list of tasks and return true to indicate that the task deletion was successful
                var json = await File.ReadAllTextAsync(FilePath).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }
                // Deserialize the json string to a list of AppTask objects, if the deserialization fails return false to indicate that the task deletion failed, if the deserialization is successful find the index of the task with the given task id in the list of tasks, if the task is not found return false to indicate that the task deletion failed, if the task is found remove it from the list of tasks and update the json file with the new list of tasks and return true to indicate that the task deletion was successful
                var tasks = JsonSerializer.Deserialize<List<AppTask>>(json) ?? new List<AppTask>();
                var index = tasks.FindIndex(x => x.id == taskId);
                if (index < 0)
                {
                    return false;
                }
                // Remove the task with the given task id from the list of tasks and update the json file with the new list of tasks and return true to indicate that the task deletion was successful
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

        // Return a list of strings that contains all the help commands for the task tracker app, each string in the list represents a command and its description, the commands include add, update, delete, mark-in-progress, mark-done, list, list done, list todo, list in-progress, exit and clear
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
            // Serialize the list of tasks to a json string and write it to the json file to update the existing tasks in the json file with the new list of tasks
            string updatedAppTasks = JsonSerializer.Serialize<List<AppTask>>(tasksFromJson.Result);
            File.WriteAllText(FilePath, updatedAppTasks);
        }

        // Read the existing tasks from the json file and return it as a list of AppTask objects, if the json file is empty or the deserialization fails return an empty list of AppTask objects
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

                // Check if the json file exists, if not return an empty list of AppTask objects, if it exists read the existing tasks from it and return it as a list of AppTask objects, if the json file is empty or the deserialization fails return an empty list of AppTask objects
                if (!File.Exists(FilePath))
                {
                    return System.Threading.Tasks.Task.FromResult(new List<Models.AppTask>());
                }

                string jsonString = File.ReadAllText(FilePath);
                // Deserialize the json string to a list of AppTask objects, if the deserialization fails return an empty list of AppTask objects, if the deserialization is successful return the list of AppTask objects
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
                //  Check if the json file exists, if not return an empty list of AppTask objects, if it exists read the existing tasks from it and filter the tasks by the given status and return the filtered list of AppTask objects, if the json file is empty or the deserialization fails return an empty list of AppTask objects
                if (!File.Exists(FilePath))
                {
                    return Task.FromResult(new List<Models.AppTask>());
                }

                string jsonString = File.ReadAllText(FilePath);
                // Deserialize the json string to a list of AppTask objects, if the deserialization fails return an empty list of AppTask objects, if the deserialization is successful filter the tasks by the given status and return the filtered list of AppTask objects
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
            // Read the existing tasks from the json file, find the task with the given task id and update its status to the given status, if the task is not found return false to indicate that the status update failed, if the task is found and updated with the new status update the json file with the new list of tasks and return true to indicate that the status update was successful
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
            // Read the existing tasks from the json file, find the task with the given task id and update its description to the given description, if the task is not found return false to indicate that the task update failed, if the task is found and updated with the new description update the json file with the new list of tasks and return true to indicate that the task update was successful
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
