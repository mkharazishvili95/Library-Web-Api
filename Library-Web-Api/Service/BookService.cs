using FluentValidation;
using Library_Web_Api.Database;
using Library_Web_Api.Models;
using Library_Web_Api.Validation;
using Microsoft.EntityFrameworkCore;

namespace Library_Web_Api.Service
{
    public interface IBookService
    {
        Task<Book> AddBook(Book newBook);
        Task<IEnumerable<Book>> GetAllBooks();
        Task<IEnumerable<Book>> GetAllBooksIncludingAuthors();
        Task<IEnumerable<Book>> GetAvailableBooks();
        Task<Book> GetBookById(int bookId);
        Task<Book> GetBookByIdIncludingAuthors(int bookId);
        Task<Book> GetBookByBookName(string bookName);
        Task<Book> GetBookByAuthorsName(string authorsName);
        Task<bool> DeleteBook(int bookId);
        Task<bool> TakingOutTheBook(int bookId);
        Task<bool> ReturnTheBook(int bookId);
    }
    public class BookService : IBookService
    {
        private readonly LibraryContext _context;
        private readonly BookValidator _bookValidator;
        public BookService(LibraryContext context, BookValidator bookValidator)
        {
            _context = context;
            _bookValidator = bookValidator;
        }

        public async Task<Book> AddBook(Book newBook)
        {
            try
            {
                var validatorResults = _bookValidator.Validate(newBook);
                if (!validatorResults.IsValid)
                {
                    return null;
                }
                else
                {
                    await _context.AddAsync(newBook);
                    await _context.SaveChangesAsync();
                    return newBook;
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> DeleteBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if (existingBook == null)
                {
                    return false;
                }
                else
                {
                    _context.Remove(existingBook);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Book>> GetAllBooks()
        {
            try
            {
                return await _context.Books.ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Book>> GetAllBooksIncludingAuthors()
        {
            try
            {
                return await _context.Books.Include(book => book.Authors).ToListAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<Book>> GetAvailableBooks()
        {
            try
            {
                var availableBooks = await _context.Books.Where(book => book.IsCheckedOut == false).ToListAsync();
                return availableBooks;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Book> GetBookByAuthorsName(string authorsName)
        {
            try
            {
                return await _context.Books.Include(book => book.Authors)
            .FirstOrDefaultAsync(book => book.Authors.Any(author => author.FirstName.ToUpper() == authorsName.ToUpper() ||
            author.LastName.ToUpper() == authorsName.ToUpper()));
            }
            catch
            {
                return null;
            }
        }

        public async Task<Book> GetBookByBookName(string bookName)
        {
            try
            {
                return await _context.Books.FirstOrDefaultAsync(book => book.Title.ToUpper().Contains(bookName.ToUpper()));
            }
            catch
            {
                return null;
            }
        }

        public async Task<Book> GetBookById(int bookId)
        {
            try
            {
                return await _context.Books.FindAsync(bookId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Book> GetBookByIdIncludingAuthors(int bookId)
        {
            try
            {
                return await _context.Books.Include(book => book.Authors).FirstOrDefaultAsync(book => book.Id == bookId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ReturnTheBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if (existingBook == null)
                {
                    return false;
                }
                if (!existingBook.IsCheckedOut)
                {
                    return false; 
                }
                existingBook.IsCheckedOut = false;
                _context.Update(existingBook);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TakingOutTheBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if (existingBook == null)
                {
                    return false;
                }
                if (existingBook.IsCheckedOut)
                {
                    return false;
                }
                existingBook.IsCheckedOut = true;
                _context.Update(existingBook);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
