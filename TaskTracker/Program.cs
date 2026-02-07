using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Interfaces;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.Utilities;

/*
 Entry point for the TaskTracker console application.

 Responsibilities:
 - Configure dependency injection and obtain an ITaskService instance.
 - Read and parse user commands in a REPL-style loop.
 - Route commands to helper functions that perform add/update/delete/list/mark operations.

 Supported commands:
 - help
 - add "task description"
 - update {id} "new description"
 - delete {id}
 - list [todo|in-progress|done]
 - mark-todo {id}
 - mark-in-progress {id}
 - mark-done {id}
 - clear
 - exit

 Notes:
 - Input parsing and most user-facing output are delegated to Utility.
 - This file uses top-level statements and local functions.
*/

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();

/* The application-level task service resolved from the DI container.
   Provides async task operations used throughout the program. */
var _taskService = serviceProvider.GetService<ITaskService>();

DisplayWelcomeMessage();

List<string> commands = [];
while (true)
{
    Utility.PrintCommandMessage("Enter command : ");
    string input = Console.ReadLine() ?? string.Empty;

    if (string.IsNullOrEmpty(input))
    {
        Utility.PrintInfoMessage("\n No input detected, Try again!");
        continue;
    }

    commands = Utility.ParseInput(input);

    string command = commands[0].ToLower();

    bool exit = false;

    switch (command)
    {
        case "help":
            PrintHelpCommands();
            break;

        case "add":
            AddNewTask();
            break;

        case "delete":
            DeleteTask();
            break;

        case "update":
            UpdateTask();
            break;

        case "list":
            DisplayAllTasks();
            break;

        case "clear":
            Utility.ClearConsole();
            DisplayWelcomeMessage();
            continue;

        case "mark-in-progress":
            SetStatusOfTask();
            break;

        case "mark-todo":
            SetStatusOfTask();
            break;

        case "mark-done":
            SetStatusOfTask();
            break;

        case "exit":
            exit = true;
            break;

        default:
            break;
    }

    if (exit)
    {
        break;
    }

}

/* Validate input and call task service to set a task's status.
   Expected forms:
   - mark-todo {id}
   - mark-in-progress {id}
   - mark-done {id} */
void SetStatusOfTask()
{
    if (!IsUserInputValid(commands, 2))
    {
        return;
    }

    int id = IsValidIdProvided(commands, 0).Item2;


    if (id == 0)
    {
        return;
    }

    var result = _taskService?.SetStatus(commands[0], id).Result;

    if (result != null && result.Value)
    {
        Utility.PrintInfoMessage($"Task status set successfully with Id : {id}");
    }
    else
    {
        Utility.PrintInfoMessage($"Task with Id : {id}, does not exist!");
    }

}

/* Display tasks in a table. Supports optional status filter:
   - list            => all tasks
   - list todo       => tasks with todo status
   - list in-progress=> tasks with in-progress status
   - list done       => tasks with done status */
void DisplayAllTasks()
{
    if (commands.Count > 2)
    {
        Utility.PrintErrorMessage("Wrong command! Try again.");
        Utility.PrintInfoMessage("Type \"help\" to know the set of commands");
        return;
    }

    List<AppTask> tasks = new List<AppTask>();
    if (commands.Count == 1)
    {
        tasks = _taskService?.GetAllTasks().Result.OrderBy(x => x.id).ToList() ?? tasks;
    }
    else
    {
        if (!commands[1].ToLower().Equals("in-progress") && !commands[1].ToLower().Equals("done") && !commands[1].ToLower().Equals("todo"))
        {
            Utility.PrintErrorMessage("Wrong command! Try again.");
            Utility.PrintInfoMessage("Type \"help\" to know the set of commands");
            return;
        }
        tasks = _taskService?.GetTasksByStatus(commands[1]).Result.OrderBy(x => x.id).ToList() ?? tasks;
    }

    CreateTaskTable(tasks);
}

/* Render a simple console table of tasks. Colors each row depending on task status.
   Columns: Task Id, Description, Status, Created Date, UpdateDate */
static void CreateTaskTable(List<AppTask> tasks)
{
    int colWidth1 = 15, colWidth2 = 35, colWidth3 = 15, colWidth4 = 15, colWidth5 = 15;
    if (tasks != null && tasks.Count > 0)
    {
        Console.WriteLine("\n{0,-" + colWidth1 + "} {1,-" + colWidth2 + "} {2,-" + colWidth3 + "} {3,-" + colWidth4 + "} {4,-" + colWidth5 + "}",
            "Task Id", "Description", "Status", "Created Date", "UpdateDate" + "\n");

        foreach (var task in tasks)
        {
            SetConsoleTextColor(task);
            Console.WriteLine("{0,-" + colWidth1 + "} {1,-" + colWidth2 + "} {2,-" + colWidth3 + "} {3,-" + colWidth4 + "} {4,-" + colWidth5 + "}"
                , task.id, task.description, task.status, task.createdAt.Date.ToString("dd-MM-yyyy"), task.updatedAt.Date.ToString("dd-MM-yyyy"));
            Console.ResetColor();
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n No Task exists! \n");
        Console.ResetColor();

        Console.WriteLine("{0,-" + colWidth1 + "} {1,-" + colWidth2 + "} {2,-" + colWidth3 + "} {3,-" + colWidth4 + "} {4,-" + colWidth5 + "}",
           "Task Id", "Description", "Status", "CreatedDate", "UpdateDate");
    }
}

/* Update the description of an existing task.
   Expected form: update {id} "new description" */
void UpdateTask()
{
    if (!IsUserInputValid(commands, 3))
    {
        return;
    }

    int id = IsValidIdProvided(commands, 0).Item2;


    if (id == 0)
    {
        return;
    }

    var result = _taskService?.UpdateTask(id, commands[2]).Result;

    if (result != null && result.Value)
    {
        Utility.PrintInfoMessage($"Task updated successfully with Id : {id}");
    }
    else
    {
        Utility.PrintInfoMessage($"Task with Id : {id}, does not exist!");
    }
}

/* Delete a task by id.
   Expected form: delete {id} */
void DeleteTask()
{
    if (!IsUserInputValid(commands, 2))
    {
        return;
    }

    int id = IsValidIdProvided(commands, 0).Item2;

    if (id == 0)
    {
        return;
    }

    var result = _taskService?.DeleteTask(id).Result;

    if (result != null && result.Value)
    {
        Utility.PrintInfoMessage($"Task deleted successfully with Id : {id}");
    }
    else
    {
        Utility.PrintInfoMessage($"Task with Id : {id}, does not exist!");
    }
}

/* Add a new task with the provided description.
   Expected form: add "task description" */
void AddNewTask()
{
    if (!IsUserInputValid(commands, 2))
    {
        return;
    }

    var taskAdded = _taskService?.AddNewTask(commands[1]);

    if (taskAdded != null && taskAdded.Result != 0)
        Utility.PrintInfoMessage($"Task added successfully with Id : {taskAdded.Result}");
    else
        Utility.PrintInfoMessage("Task not saved! Try Again");

}

/* Print the list of help commands retrieved from the task service. */
void PrintHelpCommands()
{
    var helpCommands = _taskService?.GetAllHelpCommands();
    int count = 1;
    if (helpCommands != null)
    {
        foreach (var item in helpCommands)
        {
            Utility.PrintHelpMessage(count + ". " + item);
            count++;
        }
    }
}

/* Configure application services used by this console program.
   Currently registers ITaskService => TaskService as a singleton. */
static void ConfigureServices(IServiceCollection services)
{
    // Register services here
    services.AddSingleton<ITaskService, TaskService>();
}

/* Validate parsed user input based on expected token count:
   - parameterRequired = 1 : command only
   - parameterRequired = 2 : command + single parameter (e.g., id or description)
   - parameterRequired = 3 : command + two parameters (e.g., id + description)

   On validation failure, prints guidance to the user. */
static bool IsUserInputValid(List<string> commands, int parameterRequired)
{
    bool validInput = true;

    if (parameterRequired == 1)
    {
        if (commands.Count != parameterRequired)
        {
            validInput = false;
        }
    }

    if (parameterRequired == 2)
    {
        if (commands.Count != parameterRequired || string.IsNullOrEmpty(commands[1]))
        {
            validInput = false;
        }
    }

    if (parameterRequired == 3)
    {
        if (commands.Count != parameterRequired || string.IsNullOrEmpty(commands[1]) || string.IsNullOrEmpty(commands[2]))
        {
            validInput = false;
        }
    }

    if (!validInput)
    {

        Utility.PrintErrorMessage("Wrong command! Try again.");
        Utility.PrintInfoMessage("Type \"help\" to know the set of commands");
        return false;
    }

    return true;
}

/* Attempt to parse the id parameter (commands[1]) into an integer.
   On failure (id == 0) prints an error and returns (false, 0). */
static Tuple<bool, int> IsValidIdProvided(List<string> commands, int id)
{
    Int32.TryParse(commands[1], out id);

    if (id == 0)
    {
        Utility.PrintErrorMessage("Wrong command! Try again.");
        Utility.PrintInfoMessage("Type \"help\" to know the set of commands");
        return new Tuple<bool, int>(false, id);
    }

    return new Tuple<bool, int>(true, id);
}

/* Print the initial welcome message and a hint to type "help". */
static void DisplayWelcomeMessage()
{
    Utility.PrintInfoMessage("Hello, Welcome to Task Tracker!");
    Utility.PrintInfoMessage("Type \"help\" to know the set of commands");
}

/* Set the console foreground color based on the task status:
   - todo       => Magenta
   - done       => Green
   - otherwise  => Yellow (for in-progress) */
static void SetConsoleTextColor(AppTask task)
{
    if (task.status == TaskTracker.Enums.StatusEnum.todo)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
    }
    else if (task.status == TaskTracker.Enums.StatusEnum.done)
    {
        Console.ForegroundColor = ConsoleColor.Green;
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
    }
}