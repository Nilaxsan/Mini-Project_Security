using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniTutor.Interface;
using UniTutor.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniTutor.DTO;
using Microsoft.EntityFrameworkCore;
using UniTutor.Repository;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace UniTutor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student,Admin")]
    public class TodoItemController : ControllerBase
    {
        private readonly ITodoItem _todoItem;
        private readonly IEmailService _emailService;

        public TodoItemController(ITodoItem todoItem, IEmailService emailService)
        {
            _todoItem = todoItem;
            _emailService = emailService;
        }
        private bool IsOwnerOrAdmin(int requestedId, string userType)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Admin")
                return true;

            if (role == userType && currentUserId == requestedId.ToString())
                return true;

            return false;
        }


        // GET: api/Todos/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodosByStudentId(int studentId)
        {
            if (!IsOwnerOrAdmin(studentId, "Student"))
                return Forbid("You cannot access another student's todos.");

            var todos = await _todoItem.GetByStudentIdAsync(studentId);
            return Ok(todos);
        }


        //get all todos by tutorId
        [HttpGet("tutor/{tutorId}")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodosByTutorId(int tutorId)
        {
            if (!IsOwnerOrAdmin(tutorId, "Tutor"))
                return Forbid("You cannot access another tutor's todos.");

            var todos = await _todoItem.GetByTutorIdAsync(tutorId);
            return Ok(todos);
        }


        [HttpPost("{usertype}/{id}")]
        public async Task<ActionResult<TodoItem>> PostTodo(string usertype, int id, TodoItemDto todoItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsOwnerOrAdmin(id, usertype))
                return Forbid("You cannot create todo for another user.");

            var todoItem = new TodoItem
            {
                text = todoItemDto.text,
                isCompleted = false
            };

            if (usertype == "Student")
                todoItem.studentId = id;
            else if (usertype == "Tutor")
                todoItem.tutorId = id;
            else
                return BadRequest("Invalid usertype.");

            var newTodo = await _todoItem.CreateAsync(todoItem);

            return CreatedAtAction(nameof(GetTodos), new { id = newTodo._id }, newTodo);
        }


        // GET: api/Todos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
        {
            var todos = await _todoItem.GetAllAsync();
            return Ok(todos);
        }


        // DELETE: api/Todos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var todoItem = await _todoItem.GetByIdAsync(id);

            if (todoItem == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            bool isOwner =
                (role == "Student" && todoItem.studentId.ToString() == currentUserId) ||
                (role == "Tutor" && todoItem.tutorId.ToString() == currentUserId) ||
                (role == "Admin");

            if (!isOwner)
                return Forbid("You cannot delete this todo.");

            await _todoItem.DeleteAsync(id);

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoStatus(int id)
        {
            var todoItem = await _todoItem.GetByIdAsync(id);

            if (todoItem == null)
                return NotFound();

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            bool isOwner =
                (role == "Student" && todoItem.studentId.ToString() == currentUserId) ||
                (role == "Tutor" && todoItem.tutorId.ToString() == currentUserId) ||
                (role == "Admin");

            if (!isOwner)
                return Forbid("You cannot modify this todo.");

            todoItem.isCompleted = !todoItem.isCompleted;

            await _todoItem.UpdateAsync(todoItem);

            return Ok();
        }

    }
}
