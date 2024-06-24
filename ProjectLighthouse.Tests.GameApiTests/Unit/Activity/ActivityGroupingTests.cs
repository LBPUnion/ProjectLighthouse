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

          List<OuterActivityGroup> groups = activities.ToActivityDto().AsQueryable().ToActivityGroups().ToList().ToOuterActivityGroups();
          Assert.NotNull(groups);
          Assert.Equal(3, groups.Count);
          
          Assert.Equal(ActivityGroupType.User, groups.ElementAt(0).Key.GroupType);
          Assert.Equal(ActivityGroupType.User, groups.ElementAt(1).Key.GroupType);
          Assert.Equal(ActivityGroupType.Level, groups.ElementAt(2).Key.GroupType);

          Assert.Equal(1, groups.ElementAt(0).Key.TargetUserId);
          Assert.Equal(2, groups.ElementAt(1).Key.TargetUserId);
          Assert.Equal(1, groups.ElementAt(2).Key.TargetSlotId);

          Assert.Single(groups.ElementAt(0).Groups);
          Assert.Single(groups.ElementAt(1).Groups);
          Assert.Equal(2, groups.ElementAt(2).Groups.Count);
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

          Assert.Multiple(() =>
          {
               Assert.NotNull(groups);
               Assert.Equal(2, groups.Count);
               Assert.Equal(1, groups.Count(g => g.Key.UserId == 1));
               Assert.Equal(1, groups.Count(g => g.Key.UserId == 2));
               OuterActivityGroup firstUserGroup = groups.FirstOrDefault(g => g.Key.UserId == 1);
               OuterActivityGroup secondUserGroup = groups.FirstOrDefault(g => g.Key.UserId == 2);
               Assert.NotNull(firstUserGroup.Groups);
               Assert.NotNull(secondUserGroup.Groups);

               Assert.Equal(ActivityGroupType.User, firstUserGroup.Key.GroupType);
               Assert.Equal(ActivityGroupType.User, secondUserGroup.Key.GroupType);

               Assert.True(firstUserGroup.Groups.All(g => g.Key.UserId == 1));
               Assert.True(secondUserGroup.Groups.All(g => g.Key.UserId == 2));
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