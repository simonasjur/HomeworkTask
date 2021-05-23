using System;
using System.Collections.Generic;
using VismaLibrary;
using Xunit;

namespace VismaLibraryTests
{
    public class LibraryTests
    {
        [Theory]
        [InlineData(true, "2017-01-01")]
        [InlineData(false, "2017/01/01")]
        public void IsDateValidTest(bool expected, string date)
        {
            var actual = Library.IsDateValid(date);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(FindBookData))]
        public void FindBookTest(Book expected, List<Book> books, string isbn)
        {
            var actual = Library.FindBook(books, isbn, true);

            Assert.Equal(expected.ISBN, actual.ISBN);
            Assert.Equal(expected.IsTaken, actual.IsTaken);
        }

        public static IEnumerable<object[]> FindBookData => new List<object[]>{
            new object[] { new Book("string", "string", "string", "string", "2010-10-10", "1"), 
                            new List<Book>{ new Book("string", "string", "string", "string", "2010-10-10", "1"),
                            new Book("string", "string", "string", "string", "2010-10-10", "2") }, "1"},
            new object[] { new Book("string", "string", "string", "string", "2010-10-10", "2"),
                            new List<Book>{ new Book("string", "string", "string", "string", "2010-10-10", "1"),
                            new Book("string", "string", "string", "string", "2010-10-10", "2") }, "2"}
        };


    }
}
