using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Postamat.Models;
using Postamat.Repositories;
using Postamat;
using Microsoft.AspNetCore.Authorization;

namespace TokenApp.Controllers
{
    /// <summary>
    /// Контроллер авторизации.
    /// </summary>
    public class AccountController : Controller
    {
        IRepository<User> Users;
        IRepository<Customer> Customers;
        public AccountController(IRepository<User> Users, IRepository<Customer> Customers) => (this.Users, this.Customers) = (Users, Customers);

        /// <summary>
        /// Метод для получения токена при авторизации пользователя.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("/token")]
        public IActionResult Token(string login, string password)
        {
            User person = Users.GetAll().FirstOrDefault(x => x.Login == login && x.Password == password);
            if (person == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, person.Login),
                new Claim(ClaimTypes.Role, person.Role)
            };

            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = person.Login,
                role = person.Role
            };

            return Json(response);
        }

        /// <summary>
        /// Метод для получения токена при регистрации пользователя.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("/authorize")]
        public IActionResult Authorize(string login, string password)
        {
            if (login == "" || password == "")
            {
                return BadRequest(new { errorText = "Invalid data." });
            }
            if (Users.GetAll().Any(u => u.Login == login))
            {
                return BadRequest(new { errorText = "User with this login already exists." });
            }
            var newUser = Users.Create(new User { Login = login, Password = password, Role = "customer" });
            var newCustomer = Customers.Create(new Customer());
            newUser.CustomerID = newCustomer.ID;
            Users.Update(newUser);
            return Token(newUser.Login, newUser.Password);
        }

        /// <summary>
        /// Метод, подтверждающий, что пользователь авторизирован с ролью "role".
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpGet("/role/{role}")]
        [Authorize]
        public IActionResult Role(string role) => ((ClaimsIdentity)User.Identity).Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role) ? Json(true) : Unauthorized();

        /// <summary>
        /// Метод, возвращающий зарегестированных пользователей (демонстрационный режим).
        /// </summary>
        /// <returns></returns>
        [HttpGet("/users")]
        public IActionResult GetUsers() => Ok(Users.GetAll().Select(u => u.Login).OrderBy(u => u).ToList());

        /// <summary>
        /// Метод, возвращающий пароль пользователя (демонстрационный режим).
        /// </summary>
        /// <returns></returns>
        [HttpGet("/password/{login}")]
        public IActionResult GetPassword(string login) => Ok(Users.GetAll().FirstOrDefault(u => u.Login == login).Password);
    }
}