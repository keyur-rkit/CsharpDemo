using System;
using System.Data;
using System.IO;

namespace LibraryManagement
{
    /// <summary>
    /// Represents the status of a book in the library
    /// </summary>
    public enum BookStatus
    {
        Available,
        Issued,
        Lost
    }

    /// <summary>
    /// Represents a book in the library system
    /// </summary>
    public class Book
    {
        #region Private Members
        private string _isbn;
        #endregion

        #region Public Properties
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN
        {
            get { return _isbn; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("ISBN cannot be empty");
                _isbn = value;
            }
        }
        public BookStatus Status { get; set; }
        public DateTime LastIssuedDate { get; set; }
        public string CurrentBorrower { get; set; }
        #endregion

        #region Constructors
        public Book(string title, string author, string isbn, BookStatus? bookStatus, DateTime? lastIssuedDate) 
        {
            Title = title;
            Author = author;
            ISBN = isbn;
            Status = bookStatus ?? BookStatus.Available;
            LastIssuedDate = lastIssuedDate ?? DateTime.MinValue;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Overide ToString Method for custom output
        /// </summary>
        /// <returns>Custom String Output</returns>
        public override string ToString()
        {
            return $"Title: {Title}, Author: {Author}, ISBN: {ISBN}, Status: {Status}";
        }

        /// <summary>
        /// Converts the book to a CSV format string
        /// </summary>
        /// <returns>Csv Format</returns>
        public string ToCsv()
        {
            return $"{Title},{Author},{ISBN},{Status},{LastIssuedDate:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// Creates a book from a CSV format string
        /// </summary>
        /// <param name="csvLine"></param>
        /// <returns>Book object</returns>
        /// <exception cref="FormatException"></exception>
        public static Book FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            if (values.Length >= 5)
            {
                BookStatus status = (BookStatus)Enum.Parse(typeof(BookStatus), values[3]);
                DateTime lastIssuedDate = DateTime.Parse(values[4]);

                Book book = new Book(values[0], values[1], values[2], status, lastIssuedDate);

                return book ;

            }
            throw new FormatException("Invalid CSV format");
        }
        #endregion
    }

    /// <summary>
    /// Manages the library operations and book collection
    /// </summary>
    public class Library
    {
        #region Private Members
        private List<Book> _books;
        private string _dataFile = @"F:\Keyur-417\Code\CsharpDemo\LibraryManagement\Data\books.csv";
        #endregion

        #region Constructors
        public Library()
        {
            _books = new List<Book>();
            LoadBooks();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets all books as a DataTable for reporting
        /// </summary>
        /// <returns>Table</returns>
        public DataTable GetBooksDataTable()
        {
            var table = new DataTable("Books");

            // Add columns
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Author", typeof(string));
            table.Columns.Add("ISBN", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Last Issued", typeof(DateTime));

            // Add rows
            foreach (var book in _books)
            {
                table.Rows.Add(
                    book.Title,
                    book.Author,
                    book.ISBN,
                    book.Status.ToString(),
                    book.LastIssuedDate
                );
            }

            return table;
        }

        /// <summary>
        /// Adds a new book to the library
        /// </summary>
        /// <param name="title"></param>
        /// <param name="author"></param>
        /// <param name="isbn"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddBook(string title, string author, string isbn)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(isbn))
                throw new ArgumentException("Title, Author, and ISBN are required");

            if (_books.FindAll(b => b.ISBN == isbn).Count > 0 )
                throw new InvalidOperationException("A book with this ISBN already exists");

            Book book = new Book(title, author, isbn, null, null);
            _books.Add(book);
            SaveBooks();
        }

        /// <summary>
        /// Finds a book by ISBN
        /// </summary>
        /// <param name="isbn"></param>
        /// <returns>Book or null</returns>
        public Book FindBook(string isbn)
        {
            foreach (Book book in _books)
            {
                if (book.ISBN == isbn)
                {
                    return book;
                }
            }
            return null; // Return null if no matching book is found.
        }

        /// <summary>
        /// Issues a book to a borrower
        /// </summary>
        /// <param name="isbn"></param>
        /// <param name="borrowerName"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void IssueBook(string isbn, string borrowerName)
        {
            if (string.IsNullOrEmpty(borrowerName))
                throw new ArgumentException("Borrower name is required");

            var book = FindBook(isbn) ?? throw new ArgumentException("Book not found");

            if (book.Status != BookStatus.Available)
                throw new InvalidOperationException("Book is not available for issue");

            book.Status = BookStatus.Issued;
            book.LastIssuedDate = DateTime.Now;
            book.CurrentBorrower = borrowerName;
            SaveBooks();

            // Log the transaction
            TransactionLogger.LogIssue(book, borrowerName);
        }

        /// <summary>
        /// Returns a book to the library
        /// </summary>
        /// <param name="isbn"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void ReturnBook(string isbn)
        {
            var book = FindBook(isbn) ?? throw new ArgumentException("Book not found");

            if (book.Status != BookStatus.Issued)
                throw new InvalidOperationException("Book is not issued");

            string borrowerName = book.CurrentBorrower; // Store before resetting

            book.Status = BookStatus.Available;
            book.CurrentBorrower = null;
            SaveBooks();

            // Log the transaction
            TransactionLogger.LogReturn(book);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Load books data from csv file
        /// </summary>
        /// <exception cref="IOException"></exception>
        private void LoadBooks()
        {
            string errorPath = @"F:\Keyur-417\Code\CsharpDemo\LibraryManagement\Data\error.txt";
            try
            {
                if (File.Exists(_dataFile))
                {
                    string[] lines = File.ReadAllLines(_dataFile);
                    foreach (string line in lines.Skip(1)) // Skip header row
                    {
                        try
                        {
                            Book book = Book.FromCsv(line);
                            _books.Add(book);
                        }
                        catch (Exception ex)
                        {
                            // Log invalid line and continue
                            File.AppendAllText(errorPath,
                                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error parsing line: {line}\n{ex.Message}\n\n");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Error loading books from file", ex);
            }
        }

        /// <summary>
        /// Save books data to file
        /// </summary>
        /// <exception cref="IOException"></exception>
        private void SaveBooks()
        {
            try
            {
                // Write header
                List<string> lines = new List<string>
                {
                    "Title,Author,ISBN,Status,LastIssuedDate"
                };

                // Write data
                foreach (var book in _books)
                {
                    lines.Add(book.ToCsv());
                }

                File.WriteAllLines(_dataFile, lines);
            }
            catch (Exception ex)
            {
                throw new IOException("Error saving books to file", ex);
            }
        }
        #endregion
    }

    /// <summary>
    /// Class to handle transaction logging
    /// </summary>
    public static class TransactionLogger
    {
        private static string LogFile = @"F:\Keyur-417\Code\CsharpDemo\LibraryManagement\Data\transactions.txt";

        /// <summary>
        /// Log issue book data to file
        /// </summary>
        /// <param name="book"></param>
        /// <param name="borrowerName"></param>
        public static void LogIssue(Book book, string borrowerName)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ISSUED: " +
                              $"ISBN: {book.ISBN}, Title: {book.Title}, Borrowed By: {borrowerName}\n";
            File.AppendAllText(LogFile, logMessage);
        }

        /// <summary>
        /// Log return book data to file
        /// </summary>
        /// <param name="book"></param>
        public static void LogReturn(Book book)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] RETURNED: " +
                              $"ISBN: {book.ISBN}, Title: {book.Title}\n";
            File.AppendAllText(LogFile, logMessage);
        }

        /// <summary>
        /// Print transactions data to console
        /// </summary>
        public static void GetTransactions()
        {
            using (StreamReader reader = new StreamReader(LogFile))
            {
                while (!reader.EndOfStream)
                {
                    Console.WriteLine(reader.ReadLine());
                }
            }
        }
    }
}