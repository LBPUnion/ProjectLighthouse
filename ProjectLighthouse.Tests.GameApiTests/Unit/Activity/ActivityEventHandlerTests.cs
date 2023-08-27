using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Activity;

[Trait("Category", "Unit")]
public class ActivityEventHandlerTests
{
    #region Entity Inserts
    [Fact]
    public async Task Level_Insert_ShouldCreatePublishActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SlotId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, slot);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.PublishLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task LevelComment_Insert_ShouldCreateCommentActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
        };
        database.Slots.Add(slot);

        CommentEntity comment = new()
        {
            CommentId = 1,
            PosterUserId = 1,
            TargetSlotId = 1,
            Type = CommentType.Level,
        };
        database.Comments.Add(comment);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, comment);

        Assert.NotNull(database.Activities.OfType<CommentActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.CommentOnLevel && a.CommentId == 1));
    }

    [Fact]
    public async Task ProfileComment_Insert_ShouldCreateCommentActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        CommentEntity comment = new()
        {
            CommentId = 1,
            PosterUserId = 1,
            TargetUserId = 1,
            Type = CommentType.Profile,
        };
        database.Comments.Add(comment);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, comment);

        Assert.NotNull(database.Activities.OfType<CommentActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.CommentOnUser && a.CommentId == 1));
    }

    [Fact]
    public async Task Photo_Insert_ShouldCreatePhotoActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        PhotoEntity photo = new()
        {
            PhotoId = 1,
            CreatorId = 1,
        };
        database.Photos.Add(photo);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, photo);

        Assert.NotNull(database.Activities.OfType<PhotoActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.UploadPhoto && a.PhotoId == 1));
    }

    [Fact]
    public async Task Score_Insert_ShouldCreateScoreActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);

        ScoreEntity score = new()
        {
            ScoreId = 1,
            SlotId = 1,
            UserId = 1,
        };
        database.Scores.Add(score);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, score);

        Assert.NotNull(database.Activities.OfType<ScoreActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.Score && a.ScoreId == 1));
    }

    [Fact]
    public async Task HeartedLevel_Insert_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        HeartedLevelEntity heartedLevel = new()
        {
            HeartedLevelId = 1,
            UserId = 1,
            SlotId = 1,
        };

        eventHandler.OnEntityInserted(database, heartedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.HeartLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task HeartedProfile_Insert_ShouldCreateUserActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        HeartedProfileEntity heartedProfile = new()
        {
            HeartedProfileId = 1,
            UserId = 1,
            HeartedUserId = 1,
        };

        eventHandler.OnEntityInserted(database, heartedProfile);

        Assert.NotNull(database.Activities.OfType<UserActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.HeartUser && a.TargetUserId == 1));
    }

    [Fact]
    public async Task HeartedPlaylist_Insert_ShouldCreatePlaylistActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        PlaylistEntity playlist = new()
        {
            PlaylistId = 1,
            CreatorId = 1,
        };
        database.Playlists.Add(playlist);
        await database.SaveChangesAsync();

        HeartedPlaylistEntity heartedPlaylist = new()
        {
            HeartedPlaylistId = 1,
            UserId = 1,
            PlaylistId = 1,
        };

        eventHandler.OnEntityInserted(database, heartedPlaylist);

        Assert.NotNull(database.Activities.OfType<PlaylistActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.HeartPlaylist && a.PlaylistId == 1));
    }

    [Fact]
    public async Task VisitedLevel_Insert_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        VisitedLevelEntity visitedLevel = new()
        {
            VisitedLevelId = 1,
            UserId = 1,
            SlotId = 1,
        };

        eventHandler.OnEntityInserted(database, visitedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.PlayLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task Review_Insert_ShouldCreateReviewActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);

        ReviewEntity review = new()
        {
            ReviewId = 1,
            ReviewerId = 1,
            SlotId = 1,
        };
        database.Reviews.Add(review);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, review);

        Assert.NotNull(database.Activities.OfType<ReviewActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.ReviewLevel && a.ReviewId == 1));
    }

    [Fact]
    public async Task RatedLevel_WithRatingInsert_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        RatedLevelEntity ratedLevel = new()
        {
            RatedLevelId = 1,
            UserId = 1,
            SlotId = 1,
            Rating = 1,
        };

        eventHandler.OnEntityInserted(database, ratedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.DpadRateLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task RatedLevel_WithLBP1RatingInsert_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        RatedLevelEntity ratedLevel = new()
        {
            RatedLevelId = 1,
            UserId = 1,
            SlotId = 1,
            RatingLBP1 = 5,
        };

        eventHandler.OnEntityInserted(database, ratedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.RateLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task Playlist_Insert_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        PlaylistEntity playlist = new()
        {
            PlaylistId = 1,
            CreatorId = 1,
        };
        database.Playlists.Add(playlist);
        await database.SaveChangesAsync();

        eventHandler.OnEntityInserted(database, playlist);

        Assert.NotNull(database.Activities.OfType<PlaylistActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.CreatePlaylist && a.PlaylistId == 1));
    }
    #endregion

    #region Entity changes
    [Fact]
    public async Task VisitedLevel_WithNoChange_ShouldNotCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        VisitedLevelEntity visitedLevel = new()
        {
            VisitedLevelId = 1,
            UserId = 1,
            SlotId = 1,
        };

        eventHandler.OnEntityChanged(database, visitedLevel, visitedLevel);

        Assert.Null(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.PlayLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task VisitedLevel_WithChange_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);
        await database.SaveChangesAsync();

        VisitedLevelEntity oldVisitedLevel = new()
        {
            VisitedLevelId = 1,
            UserId = 1,
            SlotId = 1,
            PlaysLBP2 = 1,
        };

        VisitedLevelEntity newVisitedLevel = new()
        {
            VisitedLevelId = 1,
            UserId = 1,
            SlotId = 1,
            PlaysLBP2 = 2,
        };

        eventHandler.OnEntityChanged(database, oldVisitedLevel, newVisitedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.PlayLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task Slot_WithTeamPickChange_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity oldSlot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(oldSlot);
        await database.SaveChangesAsync();

        SlotEntity newSlot = new()
        {
            SlotId = 1,
            CreatorId = 1,
            TeamPick = true,
        };

        eventHandler.OnEntityChanged(database, oldSlot, newSlot);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.MMPickLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task Slot_WithRepublish_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity oldSlot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };

        database.Slots.Add(oldSlot);
        await database.SaveChangesAsync();

        SlotEntity newSlot = new()
        {
            SlotId = 1,
            CreatorId = 1,
            LastUpdated = 1,
        };

        eventHandler.OnEntityChanged(database, oldSlot, newSlot);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.PublishLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task Comment_WithDeletion_ShouldCreateCommentActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        CommentEntity oldComment = new()
        {
            CommentId = 1,
            PosterUserId = 1,
            Type = CommentType.Level,
        };

        database.Comments.Add(oldComment);
        await database.SaveChangesAsync();

        CommentEntity newComment = new()
        {
            CommentId = 1,
            PosterUserId = 1,
            Type = CommentType.Level,
            Deleted = true,
        };

        eventHandler.OnEntityChanged(database, oldComment, newComment);

        Assert.NotNull(database.Activities.OfType<CommentActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.DeleteLevelComment && a.CommentId == 1));
    }

    [Fact]
    public async Task Playlist_WithSlotsChanged_ShouldCreatePlaylistWithSlotActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);

        PlaylistEntity oldPlaylist = new()
        {
            PlaylistId = 1,
            CreatorId = 1,
        };

        database.Playlists.Add(oldPlaylist);
        await database.SaveChangesAsync();

        PlaylistEntity newPlaylist = new()
        {
            PlaylistId = 1,
            CreatorId = 1,
            SlotCollection = "1",
        };

        eventHandler.OnEntityChanged(database, oldPlaylist, newPlaylist);

        Assert.NotNull(database.Activities.OfType<PlaylistWithSlotActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.AddLevelToPlaylist && a.PlaylistId == 1 && a.SlotId == 1));
    }
    #endregion

    #region Entity deletion
    [Fact]
    public async Task HeartedLevel_Delete_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);

        HeartedLevelEntity heartedLevel = new()
        {
            HeartedLevelId = 1,
            UserId = 1,
            SlotId = 1,
        };

        database.HeartedLevels.Add(heartedLevel);
        await database.SaveChangesAsync();

        eventHandler.OnEntityDeleted(database, heartedLevel);

        Assert.NotNull(database.Activities.OfType<LevelActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.UnheartLevel && a.SlotId == 1));
    }

    [Fact]
    public async Task HeartedProfile_Delete_ShouldCreateLevelActivity()
    {
        ActivityEntityEventHandler eventHandler = new();
        DatabaseContext database = await MockHelper.GetTestDatabase(new List<UserEntity>
        {
            new()
            {
                Username = "test",
                UserId = 1,
            },
        });

        SlotEntity slot = new()
        {
            SlotId = 1,
            CreatorId = 1,
        };
        database.Slots.Add(slot);

        HeartedProfileEntity heartedProfile = new()
        {
            HeartedProfileId = 1,
            UserId = 1,
            HeartedUserId = 1,
        };

        database.HeartedProfiles.Add(heartedProfile);
        await database.SaveChangesAsync();

        eventHandler.OnEntityDeleted(database, heartedProfile);

        Assert.NotNull(database.Activities.OfType<UserActivityEntity>()
            .FirstOrDefault(a => a.Type == EventType.UnheartUser && a.UserId == 1));
    }
    #endregion
}