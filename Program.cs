using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace TaskPrioritizer
{
    // Represents a task item (OOP structure)
    public class TaskItem
    {
        public string Title { get; set; }
        public string Priority { get; set; }
        public DateTime Deadline { get; set; }

        public TaskItem(string title, string priority, DateTime deadline)
        {
            Title = title;
            Priority = priority;
            Deadline = deadline;
        }
    }

    // Handles all database operations
    public class TaskService
    {
        private readonly string connectionString = "server=localhost;user=root;password=root;database=taskdb;";

        public TaskService()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"CREATE TABLE IF NOT EXISTS tasks (
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Title VARCHAR(255),
                        Priority VARCHAR(20),
                        Deadline DATE
                    );";
                    new MySqlCommand(createTableQuery, connection).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error initializing database: {ex.Message}");
            }
        }

        // Add a task to DB
        public void AddTask(TaskItem task)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO tasks (Title, Priority, Deadline) VALUES (@title, @priority, @deadline)";
                var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@title", task.Title);
                cmd.Parameters.AddWithValue("@priority", task.Priority);
                cmd.Parameters.AddWithValue("@deadline", task.Deadline);
                cmd.ExecuteNonQuery();
                Console.WriteLine("✅ Task added successfully!");
            }
        }

        // Show prioritized tasks
        public void ShowTasks()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Title, Priority, Deadline FROM tasks ORDER BY " +
                               "CASE Priority " +
                               "WHEN 'High' THEN 1 " +
                               "WHEN 'Medium' THEN 2 " +
                               "WHEN 'Low' THEN 3 " +
                               "ELSE 4 END, Deadline ASC";

                var cmd = new MySqlCommand(query, connection);
                var reader = cmd.ExecuteReader();

                Console.WriteLine("\n--- TASK PRIORITY ORDER ---");
                int count = 1;
                while (reader.Read())
                {
                    Console.WriteLine($"{count}) {reader["Title"]}  -  Priority: {reader["Priority"]}  -  Deadline: {Convert.ToDateTime(reader["Deadline"]).ToShortDateString()}");
                    count++;
                }

                if (count == 1)
                    Console.WriteLine("No tasks found.");
            }
        }
    }

    // Main program logic
    public class Program
    {
        static void Main()
        {
            var service = new TaskService();

            while (true)
            {
                Console.WriteLine("\n=== SMART TASK PRIORITIZER ===");
                Console.WriteLine("1. Add Task");
                Console.WriteLine("2. View Tasks");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddTask(service);
                        break;
                    case "2":
                        service.ShowTasks();
                        break;
                    case "3":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid choice!");
                        break;
                }
            }
        }

        static void AddTask(TaskService service)
        {
            Console.Write("Enter task title: ");
            string? title = Console.ReadLine();

            Console.Write("Enter priority (High/Medium/Low or leave blank for auto): ");
            string? priority = Console.ReadLine();

            Console.Write("Enter deadline (YYYY-MM-DD): ");
            DateTime deadline = DateTime.Parse(Console.ReadLine() ?? DateTime.Now.ToString());

            // Auto priority suggestion (optional)
            if (string.IsNullOrWhiteSpace(priority))
            {
                var daysLeft = (deadline - DateTime.Now).Days;
                if (daysLeft <= 2) priority = "High";
                else if (daysLeft <= 7) priority = "Medium";
                else priority = "Low";
            }

            var task = new TaskItem(title!, priority!, deadline);
            service.AddTask(task);
        }
    }
}
