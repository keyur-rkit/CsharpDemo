using System;
using System.Data;

namespace LibraryManagement
{
    /// <summary>
    /// Main program class containing the entry point and menu handling
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point of Library Management
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            var library = new Library();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Library Management System ===");
                Console.WriteLine("1. Add Book");
                Console.WriteLine("2. View All Books");
                Console.WriteLine("3. Search Book");
                Console.WriteLine("4. Issue Book");
                Console.WriteLine("5. Return Book");
                Console.WriteLine("6. Return Transactions");
                Console.WriteLine("7. Exit");

                Console.Write("\nEnter your choice: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddBook(library);
                        break;
                    case "2":
                        ViewBooks(library);
                        break;
                    case "3":
                        SearchBook(library);
                        break;
                    case "4":
                        IssueBook(library);
                        break;
                    case "5":
                        ReturnBook(library);
                        break;
                    case "6":
                        ViewTransactions();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid choice! Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }

        }

        /// <summary>
        /// Take inputs for adding new book
        /// </summary>
        /// <param name="library"></param>
        private static void AddBook(Library library)
        {
            Console.Clear();
            Console.WriteLine("=== Add New Book ===\n");

            Console.Write("Enter Title: ");
            string title = Console.ReadLine();

            Console.Write("Enter Author: ");
            string author = Console.ReadLine();

            Console.Write("Enter ISBN: ");
            string isbn = Console.ReadLine();

            try
            {
                library.AddBook(title, author, isbn);
                Console.WriteLine("\nBook added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Print books into tabular form
        /// </summary>
        /// <param name="library"></param>
        private static void ViewBooks(Library library)
        {
            Console.Clear();
            Console.WriteLine("=== Books Report ===\n");

            DataTable booksTable = library.GetBooksDataTable();

            // Print column headers
            foreach (DataColumn column in booksTable.Columns)
            {
                Console.Write($"{column.ColumnName,-20}");
            }
            Console.WriteLine();

            // Print rows
            foreach (DataRow row in booksTable.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    Console.Write($"{item,-20}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Take input for searching book
        /// </summary>
        /// <param name="library"></param>
        private static void SearchBook(Library library)
        {
            Console.Clear();
            Console.WriteLine("=== Search Book ===\n");

            Console.Write("Enter ISBN to search: ");
            string isbn = Console.ReadLine();

            Book book = library.FindBook(isbn);
            if (book != null)
            {
                Console.WriteLine("\nBook found:");
                Console.WriteLine(book);
            }
            else
            {
                Console.WriteLine("\nBook not found!");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Take input for Issuing book
        /// </summary>
        /// <param name="library"></param>
        private static void IssueBook(Library library)
        {
            Console.Clear();
            Console.WriteLine("=== Issue Book ===\n");

            Console.Write("Enter ISBN of book to issue: ");
            string isbn = Console.ReadLine();

            Console.Write("Enter Borrower Name: ");
            string borrowerName = Console.ReadLine();

            try
            {
                library.IssueBook(isbn, borrowerName);
                Console.WriteLine("\nBook issued successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Take input for returing book
        /// </summary>
        /// <param name="library"></param>
        private static void ReturnBook(Library library)
        {
            Console.Clear();
            Console.WriteLine("=== Return Book ===\n");

            Console.Write("Enter ISBN of book to return: ");
            string isbn = Console.ReadLine();

            try
            {
                library.ReturnBook(isbn);
                Console.WriteLine("\nBook returned successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        /// Call GetTransactions() method
        /// </summary>
        private static void ViewTransactions()
        {
            Console.Clear();
            Console.WriteLine("=== Transactions ===\n");

            TransactionLogger.GetTransactions();

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}