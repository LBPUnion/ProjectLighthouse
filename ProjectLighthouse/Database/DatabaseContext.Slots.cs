#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public async Task RemoveSlot(SlotEntity slot, bool saveChanges = true)
    {
        this.Slots.Remove(slot);

        if (saveChanges) await this.SaveChangesAsync();
    }

    public async Task HeartPlaylist(int userId, PlaylistEntity heartedPlaylist)
    {
        HeartedPlaylistEntity? heartedList = await this.HeartedPlaylists.FirstOrDefaultAsync(p => p.UserId == userId && p.PlaylistId == heartedPlaylist.PlaylistId);
        if (heartedList != null) return;

        this.HeartedPlaylists.Add(new HeartedPlaylistEntity
        {
            PlaylistId = heartedPlaylist.PlaylistId,
            UserId = userId,
        });

        await this.SaveChangesAsync();
    }

    public async Task UnheartPlaylist(int userId, PlaylistEntity heartedPlaylist)
    {
        HeartedPlaylistEntity? heartedList = await this.HeartedPlaylists.FirstOrDefaultAsync(p => p.UserId == userId && p.PlaylistId == heartedPlaylist.PlaylistId);
        if (heartedList != null) this.HeartedPlaylists.Remove(heartedList);

        await this.SaveChangesAsync();
    }

    public async Task HeartLevel(int userId, SlotEntity heartedSlot)
    {
        HeartedLevelEntity? heartedLevel = await this.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == userId && q.SlotId == heartedSlot.SlotId);
        if (heartedLevel != null) return;

        this.HeartedLevels.Add
        (
            new HeartedLevelEntity
            {
                SlotId = heartedSlot.SlotId,
                UserId = userId,
            }
        );

        await this.SaveChangesAsync();
    }

    public async Task UnheartLevel(int userId, SlotEntity heartedSlot)
    {
        HeartedLevelEntity? heartedLevel = await this.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == userId && q.SlotId == heartedSlot.SlotId);
        if (heartedLevel != null) this.HeartedLevels.Remove(heartedLevel);

        await this.SaveChangesAsync();
    }

    public async Task QueueLevel(int userId, SlotEntity queuedSlot)
    {
        QueuedLevelEntity? queuedLevel = await this.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == userId && q.SlotId == queuedSlot.SlotId);
        if (queuedLevel != null) return;

        this.QueuedLevels.Add
        (
            new QueuedLevelEntity
            {
                SlotId = queuedSlot.SlotId,
                UserId = userId,
            }
        );

        await this.SaveChangesAsync();
    }

    public async Task UnqueueLevel(int userId, SlotEntity queuedSlot)
    {
        QueuedLevelEntity? queuedLevel = await this.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == userId && q.SlotId == queuedSlot.SlotId);
        if (queuedLevel != null) this.QueuedLevels.Remove(queuedLevel);

        await this.SaveChangesAsync();
    }

}