using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VismaLibrary
{
    public class Library
    {
        const string FilePath = @"../../../../books.json";                          // File path where books informations is stored
        const int MaxBooksToTake = 3;                                               // How many maximum books a person can take from library
        const int MaxDaysToReturnBook = 60;                                         // For how many days maximum a book can be taken
        const StringComparison IgnoreCasing = StringComparison.OrdinalIgnoreCase;   // Ignore case option to use when comparing strings with Equals()

        static void Main(string[] args)
        {
            RunLibrary();
        }

        // Main method that handles flow
        static void RunLibrary()
        {
            var books = LoadBooks(FilePath);

            Console.WriteLine("Welcome to Visma’s book library!");

            while (true)
            {
                if (books.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine("Command list:");
                    Console.WriteLine("'a' - to add a new book");
                    Console.WriteLine("'t' - to take a book");
                    Console.WriteLine("'r' - to return a book");
                    Console.WriteLine("'l' - to list the books");
                    Console.WriteLine("'d' - to delete a book");
                    Console.WriteLine("'q' - to quit program");
                    Console.WriteLine();
                    Console.Write("Type a command character: ");
                    var command = Console.ReadLine();
                    switch (command)
                    {
                        case "a":
                            AddBookCommand(books);
                            break;
                        case "t":
                            TakeBookCommand(books);
                            break;
                        case "r":
                            ReturnBookCommand(books);
                            break;
                        case "l":
                            ListBooksCommand(books);
                            break;
                        case "d":
                            DeleteBookCommand(books);
                            break;
                        case "q":
                            return;
                        default:
                            Console.WriteLine($"Command {command} does not exist. Try again.");
                            Console.WriteLine();
                            break;
                    }
                }
                else // Library is empty, so starts adding new book
                {
                    Console.WriteLine("The library is empty. You should add new a book.");
                    AddBookCommand(books);
                }
            }
        }

        // Command that asks user for information about new book and then saves it to a given list and file
        static void AddBookCommand(List<Book> books)
        {
            var name = GetUserInput("new book name: ", false);
            var author = GetUserInput("new book author: ", false);
            var category = GetUserInput("new book category: ", false);
            var language = GetUserInput("new book language: ", false);
            var publicationDate = GetUserInput("new book publication date (yyyy-MM-dd): ", true);
            var isbn = GetUserInput("new book ISBN: ", false);
            books.Add(new Book(name, author, category, language, publicationDate, isbn));
            SaveBooksToFile(books, FilePath);
        }

        // Command that asks user about a book he wants to take, then updates book info if user takes it
        static void TakeBookCommand(List<Book> books)
        {
            if (!BookExists(books, true))
            {
                Console.WriteLine("All books are taken at this moment.");
                return; // return from command because all books are taken
            }
            var isbn = GetIsbnInput(books, true);
            var personName = GetUserInput("person name: ", false);

            // Keeps asking user for person name if entered person name already has max books
            while (PersonHasMoreBooks(books, personName, MaxBooksToTake))
            {
                Console.WriteLine($"Person {personName} already took {MaxBooksToTake} books.");
                personName = GetUserInput("person name: ", false);
            }

            var takenPeriodInDays = int.Parse(GetUserInput("period to take(in days): ", false));

            // Keeps asking user for taken period if entered period is too long
            while (takenPeriodInDays > MaxDaysToReturnBook)
            {
                Console.WriteLine($"Period can't be longer than {MaxDaysToReturnBook} days.");
                takenPeriodInDays = int.Parse(GetUserInput("period to take(in days): ", false));
            }

            var book = FindBook(books, isbn, true);
            book.SetAsTaken(personName, takenPeriodInDays);
            Console.WriteLine($"Book '{book.Name}' has been taken by {personName} successfully.");
            SaveBooksToFile(books, FilePath);
        }

        static void ReturnBookCommand(List<Book> books)
        {
            if (!BookExists(books, false))
            {
                Console.WriteLine("No books are taken at this moment.");
                return;     // All books are taken, return from command
            }
            var isbn = GetIsbnInput(books, false);
            var bookToReturn = FindBook(books, isbn, false);
            var personName = GetUserInput("person name: ", false);

            // Keeps looping till user enters correct returner name for a given book
            while (!bookToReturn.TakenBy.Equals(personName, IgnoreCasing))
            {
                Console.WriteLine($"Person {personName} doesn't have book with ISBN {isbn}.");
                personName = GetUserInput("person name: ", false);
            }

            // Checks whenever user returned book too late
            var message = DateTime.Today > bookToReturn.EstimatedReturnDate ?
                "At least you gave it back." : "Book has been returned successfully.";
            Console.WriteLine(message);

            bookToReturn.SetAsAvailable();
            SaveBooksToFile(books, FilePath);
        }

        // Command that lists books
        static void ListBooksCommand(List<Book> books)
        {
            Console.WriteLine("'a' - list by author");
            Console.WriteLine("'c' - list by category");
            Console.WriteLine("'l' - list by language");
            Console.WriteLine("'i' - list by ISBN");
            Console.WriteLine("'n' - list by name");
            Console.WriteLine("'av' - list of available books");
            Console.WriteLine("'t' - list of taken books");
            Console.WriteLine("Leave blank to list all books");
            Console.Write("Enter filter: ");
            var condition = Console.ReadLine();
            var filter = "";
            switch (condition)
            {
                case "a":
                    Console.Write("Enter author: ");
                    filter = Console.ReadLine();
                    break;
                case "c":
                    Console.Write("Enter category: ");
                    filter = Console.ReadLine();
                    break;
                case "l":
                    Console.Write("Enter language: ");
                    filter = Console.ReadLine();
                    break;
                case "i":
                    Console.Write("Enter ISBN: ");
                    filter = Console.ReadLine();
                    break;
                case "n":
                    Console.Write("Enter name: ");
                    filter = Console.ReadLine();
                    break;
            }
            var filteredBooks = GetFilteredBooks(books, condition, filter);
            if (!filteredBooks.Any())
            {
                Console.WriteLine("No books were found.");
                return;     // No books found so return from command
            }
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
            Console.WriteLine("Name                  | Author               | Category   | Language   | Date        | ISBN");
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
            filteredBooks.ForEach(Console.WriteLine);
            Console.WriteLine("-----------------------------------------------------------------------------------------------------");
        }

        static void DeleteBookCommand(List<Book> books)
        {
            if (!BookExists(books, true))
            {
                Console.WriteLine("All books are taken currently. No available books to delete.");
                return;     // Because all books are taken, return from command
            }
            var isbn = GetIsbnInput(books, true);
            var bookToRemove = FindBook(books, isbn, true);
            books.Remove(bookToRemove);
            SaveBooksToFile(books, FilePath);
            Console.WriteLine("Book has been removed successfully.");
        }

        // Gets ISBN from the user
        static string GetIsbnInput(List<Book> books, bool isAvailableBook)
        {
            var isbn = GetUserInput("book ISBN: ", false);
            while (!BookExists(books, isbn, isAvailableBook))
            {
                Console.WriteLine($"Book with ISBN: {isbn} doesn't exist.");
                isbn = GetUserInput("book ISBN: ", false);
            }
            return isbn;
        }

        // Keeps asking user for a input until he enters valid input (not empty/date format)
        static string GetUserInput(string inputQuery, bool checkIsDate)
        {
            Console.Write(inputQuery);
            var input = Console.ReadLine();
            while ((string.IsNullOrEmpty(input) && !checkIsDate) || (checkIsDate && !IsDateValid(input)))
            {
                Console.WriteLine($"Invalid input, try again.");
                Console.Write($"{inputQuery}: ");
                input = Console.ReadLine();
            }
            return input;
        }

        public static bool IsDateValid(string date) =>
            DateTime.TryParseExact(date, "yyyy-MM-dd", new CultureInfo("lt-LT"), DateTimeStyles.None, out _);

        // Reads books from file and then converts them from JSON to book list
        static List<Book> LoadBooks(string filePath) =>
            JsonConvert.DeserializeObject<List<Book>>(File.ReadAllText(filePath));

        // Saves book list in JSON format to a given file
        static void SaveBooksToFile(List<Book> books, string filePath) =>
            File.WriteAllText(filePath, JsonConvert.SerializeObject(books, Formatting.Indented));

        // Finds book with a given ISBN and availability
        public static Book FindBook(List<Book> books, string isbn, bool isAvailable)
        {
            return isAvailable ? books.Where(b => b.ISBN.Equals(isbn, IgnoreCasing) && !b.IsTaken).First()
               : books.Where(b => b.ISBN.Equals(isbn, IgnoreCasing) && b.IsTaken).First();
        }

        public static bool BookExists(List<Book> books, string isbn, bool isAvailable)
        {
            return isAvailable ? books.Any(b => b.ISBN is not null && b.ISBN.Equals(isbn, IgnoreCasing) && !b.IsTaken)
                : books.Any(b => b.ISBN is not null && b.ISBN.Equals(isbn, IgnoreCasing) && b.IsTaken);
        }

        public static bool BookExists(List<Book> books, bool isAvailable) => isAvailable ? books.Any(b => !b.IsTaken) : books.Any(b => b.IsTaken);

        public static bool PersonHasMoreBooks(List<Book> books, string personName, int count) =>
            books.Count(b => b.TakenBy is not null && b.TakenBy.Equals(personName, IgnoreCasing)) >= count;

        public static List<Book> GetFilteredBooks(List<Book> books, string condition, string filter)
        {
            return condition switch
            {
                "a" => books.Where(b => b.Author.Equals(filter, IgnoreCasing)).ToList(),
                "c" => books.Where(b => b.Category.Equals(filter, IgnoreCasing)).ToList(),
                "l" => books.Where(b => b.Language.Equals(filter, IgnoreCasing)).ToList(),
                "i" => books.Where(b => b.ISBN.Equals(filter, IgnoreCasing)).ToList(),
                "n" => books.Where(b => b.Name.Equals(filter, IgnoreCasing)).ToList(),
                "t" => books.Where(b => b.IsTaken).ToList(),
                "av" => books.Where(b => !b.IsTaken).ToList(),
                _ => books,
            };
        }
    }
}
