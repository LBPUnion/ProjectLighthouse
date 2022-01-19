#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types;

[XmlRoot("photo")]
[XmlType("photo")]
public class Photo
{

    [NotMapped]
    private List<PhotoSubject>? _subjects;

    [NotMapped]
    [XmlArray("subjects")]
    [XmlArrayItem("subject")]
    public List<PhotoSubject>? SubjectsXmlDontUseLiterallyEver;

    [Key]
    public int PhotoId { get; set; }

    // Uses seconds instead of milliseconds for some reason
    [XmlAttribute("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("small")]
    public string SmallHash { get; set; } = "";

    [XmlElement("medium")]
    public string MediumHash { get; set; } = "";

    [XmlElement("large")]
    public string LargeHash { get; set; } = "";

    [XmlElement("plan")]
    public string PlanHash { get; set; } = "";

    [NotMapped]
    public List<PhotoSubject> Subjects {
        get {
            if (this.SubjectsXmlDontUseLiterallyEver != null) return this.SubjectsXmlDontUseLiterallyEver;
            if (this._subjects != null) return this._subjects;

            List<PhotoSubject> response = new();
            using Database database = new();

            foreach (string idStr in this.PhotoSubjectIds.Where(idStr => !string.IsNullOrEmpty(idStr)))
            {
                if (!int.TryParse(idStr, out int id)) throw new InvalidCastException(idStr + " is not a valid number.");

                PhotoSubject? photoSubject = database.PhotoSubjects.Include(p => p.User).FirstOrDefault(p => p.PhotoSubjectId == id);
                if (photoSubject == null) continue;

                response.Add(photoSubject);
            }

            return response;
        }
        set => this._subjects = value;
    }

    [NotMapped]
    [XmlIgnore]
    public string[] PhotoSubjectIds {
        get => this.PhotoSubjectCollection.Split(",");
        set => this.PhotoSubjectCollection = string.Join(',', value);
    }

    public string PhotoSubjectCollection { get; set; } = "";

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public User? Creator { get; set; }

    public string Serialize(int slotId)
    {
        string slot = LbpSerializer.TaggedStringElement("slot", LbpSerializer.StringElement("id", slotId), "type", "user");

        string subjectsAggregate = this.Subjects.Aggregate(string.Empty, (s, subject) => s + subject.Serialize());

        string photo = LbpSerializer.StringElement("id", this.PhotoId) +
                       LbpSerializer.StringElement("small", this.SmallHash) +
                       LbpSerializer.StringElement("medium", this.MediumHash) +
                       LbpSerializer.StringElement("large", this.LargeHash) +
                       LbpSerializer.StringElement("plan", this.PlanHash) +
                       LbpSerializer.StringElement("author", this.Creator?.Username) +
                       LbpSerializer.StringElement("subjects", subjectsAggregate) +
                       slot;

        return LbpSerializer.TaggedStringElement("photo", photo, "timestamp", this.Timestamp * 1000);
    }
}