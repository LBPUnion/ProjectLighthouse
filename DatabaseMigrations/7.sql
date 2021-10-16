create table QueuedLevels
(
    QueuedLevelId int,
    UserId        int not null,
    SlotId        int not null
);

create unique index QueuedLevels_QueuedLevelId_uindex
    on QueuedLevels (QueuedLevelId);

alter table QueuedLevels
    add constraint QueuedLevels_pk
        primary key (QueuedLevelId);

alter table QueuedLevels
    modify QueuedLevelId int auto_increment;

