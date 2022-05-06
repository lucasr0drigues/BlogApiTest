using Blog.Data;
using Blog.Models;
using BlogApi.Extensions;
using BlogApi.Services;
using BlogApi.ViewModels;
using BlogApi.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace BlogApi.Controllers
{
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model, [FromServices]BlogDataContext context,[FromServices]TokenService tokenService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            }

            var user = await context.Users.AsNoTracking().Include(x => x.Roles).FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user == null)
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));

            if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            {
                return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos"));
            }

            try
            {
                var token = tokenService.GenerateToken(user);
                return Ok(new ResultViewModel<string>(token,null));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05x97 - Falha interna do servidor"));
            }
        }

        [HttpPost("v1/accounts")]
        public async Task<IActionResult> Post([FromBody] RegisterViewModel model, [FromServices] BlogDataContext context, [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Slug = model.Email.Replace("@", "-").Replace(".", "-")
            };

            var password = PasswordGenerator.Generate(25);
            user.PasswordHash = PasswordHasher.Hash(password);

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(user.Name, user.Email, "Bem vindo ao blog!", $"Sua senha é <strong>{password}</strong>");

                return Ok(new ResultViewModel<dynamic>(new {user = user.Email,password}));
            }
            catch (DbUpdateException)
            {

                return StatusCode(400, new ResultViewModel<string>("05x99 - Este e-mail já está cadastrado"));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<string>("05x98 - Falha interna do servidor"));
            }

        }

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model, [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05x077 - Falha interna do servidor"));
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name); // buscando usuário pelo email do usuário que está logado

            if (user == null)
            {
                return NotFound(new ResultViewModel<User>("Usuário não encontrado"));
            }

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultViewModel<string>("05x022 - Falha interna do servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso!", null));

        }




        //[Authorize(Roles = "user")]
        //[HttpGet("v1/user")]
        //public IActionResult GetUser() => Ok(User.Identity.Name);

            //[Authorize(Roles = "author")]
            //[HttpGet("v1/author")]
            //public IActionResult GetAuthor() => Ok(User.Identity.Name);

            //[Authorize(Roles = "admin")]
            //[HttpGet("v1/admin")]
            //public IActionResult GetAdmin() => Ok(User.Identity.Name);
    }
}
