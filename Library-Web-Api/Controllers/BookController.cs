using FluentValidation;
using Library_Web_Api.Database;
using Library_Web_Api.Models;
using Library_Web_Api.Service;
using Library_Web_Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Library_Web_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly BookValidator _bookValidator;
        private readonly LibraryContext _context;

        public BookController(IBookService bookService, BookValidator bookValidator, LibraryContext context)
        {
            _bookService = bookService;
            _bookValidator = bookValidator;
            _context = context;
        }
        [Authorize]
        [HttpPost("AddNewBook")]
        public async Task<IActionResult> AddBook([FromBody] Book newBook)
        {
            try
            {
                var validatorResults = await _bookValidator.ValidateAsync(newBook);
                if (!validatorResults.IsValid)
                {
                    return BadRequest(validatorResults.Errors);
                }
                else
                {
                    await _bookService.AddBook(newBook);
                    return Ok(new { Message = $"The book {newBook.Title} was successfully added." });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                var bookList = await _bookService.GetAllBooks();
                return Ok(bookList);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetAvailableBooks")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            try
            {
                var availableBooks = await _bookService.GetAvailableBooks();
                if(!availableBooks.Any())
                {
                    return NotFound(new { Message = $"There is no any available book." });
                }
                else
                {
                    return Ok(availableBooks);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("GetAllBooks-Including-Authors")]
        public async Task<IActionResult> GetAllBooksIncludingAuthors()
        {
            try
            {
                var bookList = await _bookService.GetAllBooksIncludingAuthors();
                return Ok(bookList);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("{bookId}/GetBookById")]
        public async Task<IActionResult> GetBookById(int bookId)
        {
            try
            {
                var existingBook = await _bookService.GetBookById(bookId);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"The book with Id: {bookId} was not found." });
                }
                return Ok(existingBook);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("{bookId}/GetBookById-including-authors")]
        public async Task<IActionResult> GetBookByIdIncludingAuthors(int bookId)
        {
            try
            {
                var existingBook = await _bookService.GetBookByIdIncludingAuthors(bookId);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"The book with Id: {bookId} was not found." });
                }
                return Ok(existingBook);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("By-Name/{bookName}")]
        public async Task<IActionResult> GetBookByBookName(string bookName)
        {
            try
            {
                var existingBook = await _bookService.GetBookByBookName(bookName);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"The book with name: {bookName} was not found." });
                }
                return Ok(existingBook);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("by-author/{authorsName}")]
        public async Task<IActionResult> GetBookByAuthorsName(string authorsName)
        {
            try
            {
                var existingBook = await _bookService.GetBookByAuthorsName(authorsName);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"The book with authors name: {authorsName} was not found." });
                }
                return Ok(existingBook);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("{bookId}/UpdateBook")]
        public async Task<IActionResult> UpdateBook(int bookId, [FromBody] Book updatedBookModel)
        {
            try
            {
                var bookValidator = new BookValidator();
                var validationResult = await bookValidator.ValidateAsync(updatedBookModel);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }
                var existingBook = await _context.Books.FindAsync(bookId);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"The book with Id: {bookId} was not found." });
                }
                existingBook.Title = updatedBookModel.Title;
                existingBook.Description = updatedBookModel.Description;
                existingBook.Image = updatedBookModel.Image;
                existingBook.Rating = updatedBookModel.Rating;
                existingBook.PublicationDate = updatedBookModel.PublicationDate;
                existingBook.IsCheckedOut = updatedBookModel.IsCheckedOut;
                existingBook.Authors = updatedBookModel.Authors;
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"The book with Id: {bookId} was successfully updated." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpDelete("{bookId}/DeleteBook")]
        public async Task<IActionResult> DeleteBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if(existingBook == null)
                {
                    return NotFound(new { Message = $"There is no any book by id: {bookId} to delete." });
                }
                else
                {
                    await _bookService.DeleteBook(bookId);
                    return Ok(new { Message = "Book has successfully deleted." });
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("{bookId}/TakingOutTheBook")]
        public async Task<IActionResult> TakingOutTheBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"There is no any book by id: {bookId} to take out." });
                }
                if (existingBook.IsCheckedOut)
                {
                    return BadRequest(new { Message = "This book is not available now." });
                }
                else
                {
                    await _bookService.TakingOutTheBook(bookId);
                    return Ok(new { Message = "The book was successfully taking out from the library." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("{bookId}/ReturnTheBook")]
        public async Task<IActionResult> ReturnTheBook(int bookId)
        {
            try
            {
                var existingBook = await _context.Books.SingleOrDefaultAsync(book => book.Id == bookId);
                if (existingBook == null)
                {
                    return NotFound(new { Message = $"There is no any book by id: {bookId} to return." });
                }
                if(!existingBook.IsCheckedOut)
                {
                    return BadRequest(new { Message = "This book is not taken out of the library." });
                }
                else
                {
                    await _bookService.ReturnTheBook(bookId);
                    return Ok(new { Message = "The book was successfully returned to the library." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
