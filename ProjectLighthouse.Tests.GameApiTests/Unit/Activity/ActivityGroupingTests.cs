using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Activity;

public class ActivityGroupingTests
{
     [Fact]
     public void ActivityGroupingTest()
     {
          List<ActivityDto> activities = new()
          {
               new ActivityDto
               {
                    TargetPlaylistId = 1,
                    Activity = new ActivityEntity(),
               },
          };
          List<OuterActivityGroup> groups = activities.AsQueryable().ToActivityGroups().ToList().ToOuterActivityGroups();
          Assert.NotNull(groups);
          Assert.Single(groups);
          OuterActivityGroup groupEntry = groups.First();
     
          Assert.Equal(ActivityGroupType.Playlist, groupEntry.Key.GroupType);
          Assert.Equal(1, groupEntry.Key.TargetId);
     }
}