create table HeartedLevels
(
    HeartedLevelId int,
    UserId        int not null,
    SlotId        int not null
);

create unique index HeartedLevels_HeartedLevelId_uindex
    on HeartedLevels (HeartedLevelId);

alter table HeartedLevels
    add constraint HeartedLevels_pk
        primary key (HeartedLevelId);

alter table HeartedLevels
    modify HeartedLevelId int auto_increment;
