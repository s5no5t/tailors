// See https://aka.ms/new-console-template for more information

using Bogus;
using Tweed.Data.Entities;

var appUsersFaker = new Faker<AppUser>()
    .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName());

var appUsers = appUsersFaker.Generate(10);

foreach (var appUser in appUsers)
    //save
    Console.WriteLine("AppUser {0} created", appUser.UserName);