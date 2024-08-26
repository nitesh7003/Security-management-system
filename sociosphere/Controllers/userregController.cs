using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sociosphere.Data;
using sociosphere.Models;
using System.Data;
using System.Net.Mail;
using System.Net;

namespace sociosphere.Controllers
{
    public class userregController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public userregController (ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(userregphoto v)
        {
            var path = _environment.WebRootPath; //getting wwwroot folder location
            var filePath = "Content/Images/" + v.photo.FileName; //placing the image
            var fullPath = Path.Combine(path, filePath); //combine the path with wwwroot folder
            UploadFile(v.photo, fullPath);

            var p = new userreg()
            {
                photo = filePath,
                name = v.name,
                email = v.email,
                city = v.city,
                role = "User",
                password = v.password
            };
            _context.Add(p);
            _context.SaveChanges();

            // Send Email After Registration
            SendRegistrationEmail(p.name, p.email, p.password);

            return RedirectToAction("Register", "userreg");
        }

        public void UploadFile(IFormFile file, string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string em, string pass)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.userregs
                    .FirstOrDefaultAsync(u =>
                        (u.email == em || u.name == em) &&
                        u.password == pass); 

                if (user != null)
                {
                    
                    var userFlatDetails = await _context.alloteflats
                        .FirstOrDefaultAsync(f => f.name == user.name);

                    if (userFlatDetails != null)
                    {
                        HttpContext.Session.SetString("WingName", userFlatDetails.wingname);
                        HttpContext.Session.SetString("FlatNo", userFlatDetails.flatno.ToString());
                    }
                    



                    HttpContext.Session.SetString("Role", user.role);
                    HttpContext.Session.SetString("name", user.name);
                    HttpContext.Session.SetString("Email", user.email);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private void SendRegistrationEmail(string name, string email, string password)
        {
            try
            {
                var fromAddress = new MailAddress("pruthvirajgavali2001@gmail.com", "SocioSphere");
                var toAddress = new MailAddress(email, name);
                const string fromPassword = "zuoddhlazdzterrq";
                const string subject = "Welcome to SocioSphere";
                string body = $"Dear {name},\n\nYour registration was successful.\n\nEmail: {email}\nPassword: {password}\n\n" +
                  "Please do not reply to this mail as it is a computer-generated mail. If you have any query, meet in the office.\n\nRegards,\nSocioSphere Team";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // Replace with your SMTP server
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle error
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }


    }
}
