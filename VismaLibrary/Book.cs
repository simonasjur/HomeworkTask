using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VismaLibrary
{
    public class Book
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public string PublicationDate { get; set; }
        public string ISBN { get; set; }
        public bool IsTaken { get; set; }
        public string TakenBy { get; set; }
        public DateTime EstimatedReturnDate { get; set; }

        public Book() { }

        public Book(string name, string author, string category, string language, string publicationDate, string isbn)
        {
            Name = name;
            Author = author;
            Category = category;
            Language = language;
            PublicationDate = publicationDate;
            ISBN = isbn;
        }

        public override string ToString()
        {
            return $"{Name, -21} | {Author, -20} | {Category, -10} | {Language, -10} | {PublicationDate, -11} | {ISBN}";
        }

        public void SetAsTaken(string takenBy, int days)
        {
            IsTaken = true;
            TakenBy = takenBy;
            EstimatedReturnDate = DateTime.Today.AddDays(days);
        }

        public void SetAsAvailable()
        {
            IsTaken = false;
            TakenBy = null;
        }
    }
}
