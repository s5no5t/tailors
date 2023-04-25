using Bogus;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Session;
using Tweed.Data.Entities;

namespace Tweed.Data.GenerateFakes;

public static class FakesCreator
{
    internal static async Task<List<TweedIdentityUser>> CreateFakeIdentityUsers(IConfigurationRoot config1,
        BulkInsertOperation bulkInsertOperation)
    {
        var identityUserFaker = new Faker<TweedIdentityUser>()
            .RuleFor(u => u.UserName, (f, _) => f.Internet.UserName())
            .RuleFor(u => u.Email, (f, _) => f.Internet.ExampleEmail());

        var numIdentityUsers = config1.GetValue<int>("NumberOfAppUsers");
        var identityUsers = identityUserFaker.Generate(numIdentityUsers);
        foreach (var identityUser in identityUsers) await bulkInsertOperation.StoreAsync(identityUser);
        Console.WriteLine("{0} IdentityUser instances created", identityUsers.Count);
        return identityUsers;
    }

    public static async Task<List<TweedUser>> CreateFakeTweedUsers(IConfigurationRoot config,
        BulkInsertOperation bulkInsert, List<TweedIdentityUser> identityUsers)
    {
        var tweedUserFaker = new Faker<TweedUser>();
        var tweedUsers = tweedUserFaker.Generate(identityUsers.Count);
        for (int index = 0; index < identityUsers.Count; index++)
        {
            tweedUsers[index].IdentityUserId = identityUsers[index].Id;
        }

        foreach (var tweedUser in tweedUsers) await bulkInsert.StoreAsync(tweedUser);

        Console.WriteLine("{0} TweedUser instances created", tweedUsers.Count);
        return tweedUsers;
    }

    internal static async Task CreateFakeFollows(List<TweedUser> list, IAsyncDocumentSession asyncDocumentSession)
    {
        var followsFaker = new Faker<Follows>()
            .RuleFor(f => f.LeaderId, f => f.PickRandom(list).Id)
            .RuleFor(f => f.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()));

        var tweedUserFollowsFaker = new Faker<TweedUser>()
            .RuleFor(u => u.Follows, f => followsFaker.GenerateBetween(0, list.Count - 1));

        foreach (var appUser in list)
        {
            tweedUserFollowsFaker.Populate(appUser);
            await asyncDocumentSession.StoreAsync(appUser);
        }
    }

    internal static async Task<List<Tweed.Data.Entities.Tweed>> CreateTweeds(List<TweedIdentityUser> list,
        IConfigurationRoot config1,
        BulkInsertOperation bulkInsertOperation)
    {
        var tweedFaker = new Faker<Tweed.Data.Entities.Tweed>()
            .RuleFor(t => t.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()))
            .RuleFor(t => t.Text, f => f.Lorem.Paragraph(1))
            .RuleFor(t => t.AuthorId, f => f.PickRandom(list).Id);

        var numTweeds = config1.GetValue<int>("NumberOfTweeds");
        var tweeds1 = tweedFaker.Generate(numTweeds);
        foreach (var tweed in tweeds1) await bulkInsertOperation.StoreAsync(tweed);
        Console.WriteLine("{0} Tweed instances created", tweeds1.Count);
        return tweeds1;
    }

    internal static async Task CreateLikes(List<Tweed.Data.Entities.Tweed> list, List<TweedUser> tweedUsers,
        IAsyncDocumentSession asyncDocumentSession)
    {
        var likesFaker = new Faker<TweedLike>()
            .RuleFor(f => f.TweedId, f => f.PickRandom(list).Id)
            .RuleFor(f => f.CreatedAt, f => DateHelper.DateTimeToZonedDateTime(f.Date.Past()));

        var tweedUserLikesFaker = new Faker<TweedUser>()
            .RuleFor(u => u.Likes, f => likesFaker.GenerateBetween(0, list.Count - 1));

        foreach (var tweedUser in tweedUsers)
        {
            tweedUserLikesFaker.Populate(tweedUser);
            await asyncDocumentSession.StoreAsync(tweedUser);
        }
    }
}