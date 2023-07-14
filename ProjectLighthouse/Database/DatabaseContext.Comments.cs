#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public async Task<bool> RateComment(int userId, int commentId, int rating)
    {
        CommentEntity? comment = await this.Comments.FirstOrDefaultAsync(c => commentId == c.CommentId);

        if (comment == null) return false;
        if (comment.PosterUserId == userId) return false;

        if (await this.IsUserBlockedBy(userId, comment.PosterUserId)) return false;
        
        RatedCommentEntity? reaction = await this.RatedComments.FirstOrDefaultAsync(r => r.UserId == userId && r.CommentId == commentId);
        if (reaction == null)
        {
            RatedCommentEntity newReaction = new()
            {
                UserId = userId,
                CommentId = commentId,
                Rating = 0,
            };
            this.RatedComments.Add(newReaction);
            await this.SaveChangesAsync();
            reaction = newReaction;
        }

        rating = Math.Clamp(rating, -1, 1);

        int oldRating = reaction.Rating;
        if (oldRating == rating) return true;

        reaction.Rating = rating;
        // if rating changed then we count the number of reactions to ensure accuracy
        List<RatedCommentEntity> reactions = await this.RatedComments.Where(c => c.CommentId == commentId).ToListAsync();
        int yay = 0;
        int boo = 0;
        foreach (RatedCommentEntity r in reactions)
        {
            switch (r.Rating)
            {
                case -1:
                    boo++;
                    break;
                case 1:
                    yay++;
                    break;
            }
        }

        comment.ThumbsDown = boo;
        comment.ThumbsUp = yay;
        await this.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PostComment(int userId, int targetId, CommentType type, string message)
    {
        if (message.Length > 100) return false;

        if (type == CommentType.Profile)
        {
            int targetUserId = await this.Users.Where(u => u.UserId == targetId)
                .Where(u => u.CommentsEnabled)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
            if (targetUserId == 0) return false;
            if (await this.IsUserBlockedBy(userId, targetUserId)) return false;
        }
        else
        {
            int creatorId = await this.Slots.Where(s => s.SlotId == targetId)
                .Where(s => s.CommentsEnabled && !s.Hidden)
                .Select(s => s.CreatorId)
                .FirstOrDefaultAsync();
            if (creatorId == 0) return false;
            
            if (await this.IsUserBlockedBy(userId, creatorId)) return false;
        }

        this.Comments.Add(new CommentEntity
        {
            PosterUserId = userId,
            TargetUserId = type == CommentType.Profile ? targetId : null,
            TargetSlotId = type == CommentType.Level ? targetId : null,
            Type = type,
            Message = message,
            Timestamp = TimeHelper.TimestampMillis,
        });
        await this.SaveChangesAsync();

        return true;
    }
}