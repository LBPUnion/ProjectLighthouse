using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Activity;

[Trait("Category", "Unit")]
public class ActivityGroupingTests
{
     [Fact]
     public void ToOuterActivityGroups_ShouldCreateGroupPerObject_WhenGroupedBy_ObjectThenActor()
     {
          List<ActivityEntity> activities = [
               new LevelActivityEntity
               {
                    UserId = 1,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.PlayLevel,
               },
               new LevelActivityEntity
               {
                    UserId = 1,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.ReviewLevel,
               },
               new LevelActivityEntity
               {
                    UserId = 2,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.PlayLevel,
               },
               new UserActivityEntity
               {
                    TargetUserId = 2,
                    UserId = 1,
                    Type = EventType.HeartUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 2,
                    UserId = 1,
                    Type = EventType.CommentOnUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 1,
                    UserId = 2,
                    Type = EventType.HeartUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 1,
                    UserId = 2,
                    Type = EventType.CommentOnUser,
                    Timestamp = DateTime.Now,
               },
          ];

          //TODO: fix test
          List<OuterActivityGroup> groups = activities.ToActivityDto().AsQueryable().ToActivityGroups().ToList().ToOuterActivityGroups();
          Assert.NotNull(groups);
          Assert.Single(groups);
          OuterActivityGroup outerGroup = groups.First();

          Assert.Equal(ActivityGroupType.Level, outerGroup.Key.GroupType);
          Assert.Equal(1, outerGroup.Key.TargetSlotId);

          IGrouping<InnerActivityGroup, ActivityDto>? firstGroup = outerGroup.Groups.First();
          IGrouping<InnerActivityGroup, ActivityDto>? secondGroup = outerGroup.Groups.Last();

          Assert.NotNull(secondGroup);
          Assert.Equal(ActivityGroupType.User, secondGroup.Key.Type);
          Assert.Equal(1, secondGroup.Key.TargetId); // user group should have the user id
          Assert.Equal(1, secondGroup.ToList()[0].TargetSlotId); // events in user group should have t
          Assert.Equal(1, secondGroup.ToList()[1].TargetSlotId);

          Assert.NotNull(firstGroup);
          Assert.Equal(ActivityGroupType.User, firstGroup.Key.Type);
          Assert.Equal(2, firstGroup.Key.TargetId);
          Assert.Equal(1, firstGroup.ToList()[0].TargetSlotId);
     }

     [Fact]
     public void ToOuterActivityGroups_ShouldCreateGroupPerObject_WhenGroupedBy_ActorThenObject()
     {
                    List<ActivityEntity> activities = [
               new LevelActivityEntity
               {
                    UserId = 1,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.PlayLevel,
               },
               new LevelActivityEntity
               {
                    UserId = 1,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.ReviewLevel,
               },
               new LevelActivityEntity
               {
                    UserId = 2,
                    SlotId = 1,
                    Slot = new SlotEntity
                    {
                         GameVersion = GameVersion.LittleBigPlanet2,
                    },
                    Timestamp = DateTime.Now,
                    Type = EventType.PlayLevel,
               },
               new UserActivityEntity
               {
                    TargetUserId = 2,
                    UserId = 1,
                    Type = EventType.HeartUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 2,
                    UserId = 1,
                    Type = EventType.CommentOnUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 1,
                    UserId = 2,
                    Type = EventType.HeartUser,
                    Timestamp = DateTime.Now,
               },
               new UserActivityEntity
               {
                    TargetUserId = 1,
                    UserId = 2,
                    Type = EventType.CommentOnUser,
                    Timestamp = DateTime.Now,
               },
          ];
          List<OuterActivityGroup> groups = activities.ToActivityDto()
               .AsQueryable()
               .ToActivityGroups(true)
               .ToList()
               .ToOuterActivityGroups(true);
          //TODO: fix test
          Assert.Multiple(() =>
          {
               Assert.NotNull(groups);
               Assert.Equal(2, groups.Count);
               OuterActivityGroup firstUserGroup = groups.FirstOrDefault(g => g.Key.UserId == 1);
               OuterActivityGroup secondUserGroup = groups.FirstOrDefault(g => g.Key.UserId == 2);
               Assert.NotNull(firstUserGroup.Groups);
               Assert.NotNull(secondUserGroup.Groups);

               Assert.Equal(ActivityGroupType.User, firstUserGroup.Key.GroupType);
               Assert.Equal(ActivityGroupType.User, secondUserGroup.Key.GroupType);

               Assert.Single(firstUserGroup.Groups);
               Assert.Single(secondUserGroup.Groups);

               Assert.Equal(2, firstUserGroup.Groups.ToList()[0].Count());
               Assert.Single(secondUserGroup.Groups.ToList()[0]);

               // Assert.Equal(ActivityGroupType.Level, outerGroup.Key.GroupType);
               // Assert.Equal(1, outerGroup.Key.TargetSlotId);
               //
               // IGrouping<InnerActivityGroup, ActivityDto>? firstGroup = outerGroup.Groups.First();
               // IGrouping<InnerActivityGroup, ActivityDto>? secondGroup = outerGroup.Groups.Last();
               //
               // Assert.NotNull(secondGroup);
               // Assert.Equal(ActivityGroupType.User, secondGroup.Key.Type);
               // Assert.Equal(1, secondGroup.Key.TargetId); // user group should have the user id
               // Assert.Equal(1, secondGroup.ToList()[0].TargetSlotId); // events in user group should have t
               // Assert.Equal(1, secondGroup.ToList()[1].TargetSlotId);
               //
               // Assert.NotNull(firstGroup);
               // Assert.Equal(ActivityGroupType.User, firstGroup.Key.Type);
               // Assert.Equal(2, firstGroup.Key.TargetId);
               // Assert.Equal(1, firstGroup.ToList()[0].TargetSlotId);
          });
     }

     [Fact]
     public async Task ToActivityDtoTest()
     {
          DatabaseContext db = await MockHelper.GetTestDatabase();
          db.Slots.Add(new SlotEntity
          {
               SlotId = 1,
               CreatorId = 1,
               GameVersion = GameVersion.LittleBigPlanet2,
          });
          db.Slots.Add(new SlotEntity
          {
               SlotId = 2,
               CreatorId = 1,
               GameVersion = GameVersion.LittleBigPlanet2,
               TeamPickTime = 1,
          });
          db.Reviews.Add(new ReviewEntity
          {
               Timestamp = DateTime.Now.ToUnixTimeMilliseconds(),
               SlotId = 1,
               ReviewerId = 1,
               ReviewId = 1,
          });
          db.Comments.Add(new CommentEntity
          {
               TargetSlotId = 1,
               PosterUserId = 1,
               Message = "comment on level test",
               CommentId = 1,
          });
          db.Comments.Add(new CommentEntity
          {
               TargetUserId = 1,
               PosterUserId = 1,
               Message = "comment on user test",
               CommentId = 2,
          });
          db.WebsiteAnnouncements.Add(new WebsiteAnnouncementEntity
          {
               PublisherId = 1,
               AnnouncementId = 1,
          });
          db.Playlists.Add(new PlaylistEntity
          {
               PlaylistId = 1,
               CreatorId = 1,
          });
          db.Activities.Add(new LevelActivityEntity
          {
               Timestamp = DateTime.Now,
               SlotId = 1,
               Type = EventType.PlayLevel,
               UserId = 1,
          });
          db.Activities.Add(new ReviewActivityEntity
          {
               Timestamp = DateTime.Now,
               SlotId = 1,
               Type = EventType.ReviewLevel,
               ReviewId = 1,
               UserId = 1,
          });
          db.Activities.Add(new UserCommentActivityEntity
          {
               Timestamp = DateTime.Now,
               Type = EventType.CommentOnUser,
               UserId = 1,
               TargetUserId = 1,
               CommentId = 2,
          });
          db.Activities.Add(new LevelCommentActivityEntity
          {
               Timestamp = DateTime.Now,
               Type = EventType.CommentOnLevel,
               UserId = 1,
               SlotId = 1,
               CommentId = 1,
          });
          db.Activities.Add(new NewsActivityEntity
          {
               Type = EventType.NewsPost,
               NewsId = 1,
               UserId = 1,
          });
          db.Activities.Add(new PlaylistActivityEntity
          {
               Type = EventType.CreatePlaylist,
               PlaylistId = 1,
               UserId = 1,
          });
          db.Activities.Add(new LevelActivityEntity
          {
               Type = EventType.MMPickLevel,
               SlotId = 2,
               UserId = 1,
          });
          await db.SaveChangesAsync();

          var sql = db.Activities.ToActivityDto().ToQueryString();

          List<ActivityDto> resultDto = await db.Activities.ToActivityDto(includeSlotCreator: true, includeTeamPick: true).ToListAsync();

          Assert.Equal(2, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.MMPickLevel)?.TargetTeamPickId);
          Assert.Equal(2, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.MMPickLevel)?.TargetSlotId);
          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.MMPickLevel)?.TargetSlotCreatorId);

          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.CreatePlaylist)?.TargetPlaylistId);

          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.NewsPost)?.TargetNewsId);

          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.CommentOnUser)?.TargetUserId);

          Assert.Null(resultDto.FirstOrDefault(a => a.Activity.Type == EventType.CommentOnLevel)?.TargetTeamPickId);
          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.CommentOnLevel)?.TargetSlotId);
          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.CommentOnLevel)?.TargetSlotCreatorId);

          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.ReviewLevel)?.TargetSlotId);
          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.ReviewLevel)?.TargetSlotCreatorId);

          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.PlayLevel)?.TargetSlotId);
          Assert.Equal(1, resultDto.FirstOrDefault(a => a.Activity.Type == EventType.PlayLevel)?.TargetSlotCreatorId);
     }
}