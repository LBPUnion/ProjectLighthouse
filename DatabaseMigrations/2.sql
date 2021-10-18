create table Slots
(
    SlotId          int,
    CreatorId       int                not null,
    Name            text               not null,
    Description     text               not null,
    IconHash        text               not null,
    RootLevel       text               not null,
    Resource        text               not null,
    LocationId      int                not null,
    InitiallyLocked bool default false not null,
    SubLevel        bool default false null,
    Lbp1Only        bool default false not null,
    Shareable       int  default 0     not null,
    AuthorLabels    text               not null,
    BackgroundHash  text               not null,
    MinimumPlayers  int  default 1     not null,
    MaximumPlayers  int  default 4     not null,
    MoveRequired    bool default false null
);

create unique index Slots_SlotId_uindex
    on Slots (SlotId);

alter table Slots
    add constraint Slots_pk
        primary key (SlotId);

alter table Slots
    modify SlotId int auto_increment;

alter table Slots
    alter column CreatorId set default -1;

alter table Slots
    modify CreatorId int not null after LocationId;

