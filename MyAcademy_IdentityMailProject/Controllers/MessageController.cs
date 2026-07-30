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
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.fullName = user.FirstName + " " + user.LastName;

            var messages = await _context.UserMessages.Include(x => x.Sender).Where
                          (x => x.ReceiverId == user.Id).ToListAsync();

            return View(messages);
        }
        public IActionResult SendMail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendMail(SendMailDto sendMailDto)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(sendMailDto.ReceiverMail);
            if (receiver == null)
            {
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
    }
}
