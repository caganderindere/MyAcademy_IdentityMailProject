using IdentityMail.Web.Context;
using IdentityMail.Web.DTOs.UserDtos.UserMessageDtos;
using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Controllers
{
    [Authorize]
    public class MessageController(UserManager<AppUser> _userManager,
                                                         AppDbContext _context) : Controller
    {
        public async Task<IActionResult> Index(string? filter, int? categoryId, string? sort)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;

            var query = _context.UserMessages
    .Include(x => x.Sender)
    .Include(x => x.Category)
    .Where(x => x.ReceiverId == user.Id && !x.IsDeleted);
            switch (filter)
            {
                case "read":
                    query = query.Where(x => x.IsRead);
                    break;

                case "unread":
                    query = query.Where(x => !x.IsRead);
                    break;

                case "starred":
                    query = query.Where(x => x.IsStarred);
                    break;
            }
            switch (sort)
            {
                case "new":
                    query = query.OrderByDescending(x => x.SendDate);
                    break;

                case "old":
                    query = query.OrderBy(x => x.SendDate);
                    break;

                case "az":
                    query = query.OrderBy(x => x.Subject);
                    break;

                case "za":
                    query = query.OrderByDescending(x => x.Subject);
                    break;

                default:
                    query = query.OrderByDescending(x => x.SendDate);
                    break;
            }
            ViewBag.SelectedSort = sort;
            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.SelectedCategory = categoryId;


            var messages = await query.ToListAsync();
            return View(messages);
        }
        public IActionResult SendMail()
        {
            ViewBag.Categories = _context.Categories.ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            Console.WriteLine(sendMailDto.ReceiverMail);
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
            if (receiver == null)
            {
                ViewBag.Categories = _context.Categories.ToList();

                ModelState.AddModelError("", "Alıcı bulunamadı.");

                return View(sendMailDto);
            }

            var NewMessage = new UserMessage
            {
                SendDate = DateTime.Now,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                Subject = sendMailDto.Subject,
                Body = sendMailDto.Body,
                CategoryId = sendMailDto.CategoryId
            };
            _context.UserMessages.Add(NewMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> MailDetail(int id)
        {
            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (message == null)
            {
                return NotFound();
            }

            message.IsRead = true;

            await _context.SaveChangesAsync();

            return View(message);
        }

        public async Task<IActionResult> ToggleStar(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            message.IsStarred = !message.IsStarred;
            await _context.SaveChangesAsync();
            return Ok();
        }
        public async Task<IActionResult> StarredMessages()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;

            var messages = await _context.UserMessages.Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id
         && x.IsStarred
         && !x.IsDeleted)
                .ToListAsync();
            return View(messages);
        }
        public async Task<IActionResult> SentMessages()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;

            var messages = await _context.UserMessages
                .Include(x => x.Receiver)
               .Where(x => x.SenderId == user.Id
                                       && !x.IsDeleted)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }
        public async Task<IActionResult> Reply(int id)
        {
            var message = await _context.UserMessages
                .Include(x => x.Sender)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (message == null)
            {
                return NotFound();
            }
            var model = new SendMailDto
            {
                ReceiverMail = message.Sender.Email
            };
            TempData["ReceiverMail"] = message.Sender.Email;

            return RedirectToAction(nameof(SendMail));
        }
        public async Task<IActionResult> MoveToTrash(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            message.IsDeleted = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Trash()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;
            ViewBag.ProfileImageUrl = user.ProfileImageUrl;

            var messages = await _context.UserMessages
                .Include(x => x.Sender)
                .Where(x => x.ReceiverId == user.Id && x.IsDeleted)
                .OrderByDescending(x => x.SendDate)
                .ToListAsync();

            return View(messages);
        }
        public async Task<IActionResult> Restore(int id)
        {
            var message = await _context.UserMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            message.IsDeleted = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Trash));
        }
    }
}
