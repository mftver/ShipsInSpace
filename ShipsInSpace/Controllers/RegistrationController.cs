﻿using System;
using Microsoft.AspNetCore.Mvc;
using ShipsInSpace.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ShipsInSpace.Controllers
{
    [Authorize(Policy = "Manager")]
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;

        public RegistrationController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Plate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Plate(RegistrationViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction("Plate");

            // Generate password
            model.SecretCode = GenerateRandomPassword();

            var user = new User {UserName = model.Plate};

            // Register user
            var result = await _userManager.CreateAsync(user, model.SecretCode);
            if(!result.Succeeded) RedirectToAction("Plate");

            // Assign claim
            var result1 = await _userManager.AddClaimAsync(user, new Claim("License", model.PilotLicense.ToString()));
            if (!result1.Succeeded) RedirectToAction("Plate");
            
            return View("RegistrationComplete",model);
        }

        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        /// Source: https://stackoverflow.com/a/46229180/10557332
        private static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            var randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            var rand = new Random(Environment.TickCount);
            var chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (var i = chars.Count; i < opts.RequiredLength
                                      || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                var rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}
