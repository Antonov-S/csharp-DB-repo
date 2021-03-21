namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            System.Console.WriteLine(RemoveBooks(db));

            //RemoveBooks(db);
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var booksToBeRemoved = context.Books
                .Where(x => x.Copies < 4200)
                .ToArray();

            context.Books.RemoveRange(booksToBeRemoved);

            context.SaveChanges();

            return booksToBeRemoved.Count();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var pricesToBeIncrease = context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToArray();

            foreach (var item in pricesToBeIncrease)
            {
                item.Price += 5;
            }

            context.SaveChanges();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var topThreeBooksBycategories = context.Categories
                .Select(category => new
                {
                    CatName = category.Name,
                    Books = category.CategoryBooks.Select(b => new
                    {
                        b.Book.Title,
                        b.Book.ReleaseDate.Value
                    })
                    .OrderByDescending(b => b.Value)
                    .Take(3)
                    .ToArray()
                })
                .OrderBy(x => x.CatName)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var categorie in topThreeBooksBycategories)
            {
                sb.AppendLine($"--{categorie.CatName}");

                foreach (var book in categorie.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var profitByCategorys = context.Categories
                .Select(x => new
                {
                    x.Name,
                    TotalProfitByCategory = x.CategoryBooks.Sum(b => b.Book.Price * b.Book.Copies)
                })
                .OrderByDescending(x => x.TotalProfitByCategory)
                .ThenBy(x => x.Name)
                .ToArray();

            var result = string.Join(Environment.NewLine, profitByCategorys.Select(x => $"{x.Name} ${x.TotalProfitByCategory:f2}"));

            return result;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var totalBooksByAutor = context.Authors
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    TotalCopies = x.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(x => x.TotalCopies)
                .ToArray();

            var result = string.Join(Environment.NewLine, totalBooksByAutor.Select(x => $"{x.FirstName} {x.LastName} - {x.TotalCopies}"));

            return result;
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var bookByCount = context.Books
                .Where(book => book.Title.Length > lengthCheck)
                .ToArray();

            return bookByCount.Length;
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var booksByauthor = context.Books
                .Where(book => EF.Functions.Like(book.Author.LastName,
                $"{input}%"))
                .Select(book => new
                {
                    book.BookId,
                    book.Title,
                    AuthorName = book.Author.FirstName + " " + book.Author.LastName 
                })
                .OrderBy(x => x.BookId)
                .ToArray();

            var result = string.Join(Environment.NewLine, booksByauthor.Select(book => $"{book.Title} ({book.AuthorName})"));

            return result;
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var bookByTitles = context.Books
                .Where(x => EF.Functions.Like(x.Title, $"%{input}%"))
                .Select(x => new {
                    x.Title
                })
                .OrderBy(x => x.Title)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var bookByTitle in bookByTitles)
            {
                sb.AppendLine($"{bookByTitle.Title}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var autors = context.Authors
                .Where(x => x.FirstName.EndsWith(input))
                .Select(x => new
                {
                    FullName = x.FirstName + " " + x.LastName
                })
                .OrderBy(x => x.FullName)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var autor in autors)
            {
                sb.AppendLine($"{autor.FullName}");
            }

            return sb.ToString().TrimEnd(); 
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var dateInDateTime = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var publishedBooksOnThisDate = context.Books
                .Where(x => x.ReleaseDate.Value < dateInDateTime)
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price,
                    x.ReleaseDate
                })
                .OrderByDescending(x => x.ReleaseDate)
                .ToList();

            //var result = string.Join(Environment.NewLine, publishedBooksOnThisDate);
            //return result;

            StringBuilder sb = new StringBuilder();

            foreach (var book in publishedBooksOnThisDate)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToArray();

            var books = context.Books
                .Include(x => x.BookCategories)
                .ThenInclude(x => x.Category)
                .ToArray()
                .Where(book => book.BookCategories
                .Any(category => categories.Contains(category.Category.Name.ToLower())))
                .Select(x => x.Title)
                .OrderBy(title => title)
                .ToArray();

            var result = string.Join(Environment.NewLine, books);
            return result;
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year != year)
                .Select(x => new
                {
                    x.BookId,
                    x.Title
                })
                .OrderBy(x => x.BookId)
                .ToArray();
            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(books => books.Price > 40m)
                .Select(x => new
                {
                    x.Title,
                    x.Price
                })
                .OrderByDescending(x => x.Price)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(books => books.EditionType == EditionType.Gold && books.Copies < 5000)
                .Select(book => new
                {
                    book.BookId,
                    book.Title
                })
                .OrderBy(x => x.BookId)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book.Title);
            }
            return sb.ToString().TrimEnd();
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var ageRestriction = Enum.Parse<AgeRestriction>(command, true);


            var books = context.Books
                .Where(x => x.AgeRestriction == ageRestriction)
                .Select(book => book.Title)
                .OrderBy(title => title)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine(book);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
