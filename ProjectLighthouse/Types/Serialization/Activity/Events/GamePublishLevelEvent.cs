﻿using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GamePublishLevelEvent : GameEvent
{
    [XmlElement("object_slot_id")]
    public ReviewSlot Slot { get; set; }

    [XmlElement("republish")]
    public int IsRepublish { get; set; }

    [XmlElement("count")]
    public int Count { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        SlotEntity slot = await database.Slots.FindAsync(this.Slot.SlotId);
        if (slot == null) return;

        this.Slot = ReviewSlot.CreateFromEntity(slot);
        // TODO does this work?
        bool republish = Math.Abs(this.Timestamp - slot.FirstUploaded) > 5000;
        this.IsRepublish = Convert.ToInt32(republish);
    }
}