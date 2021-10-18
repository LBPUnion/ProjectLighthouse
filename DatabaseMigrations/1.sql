create table Locations
(
    Id int not null,
    X  int not null,
    Y  int not null
);

create unique index Locations_UserId_uindex
    on Locations (Id);

alter table Locations
    add constraint Locations_pk
        primary key (Id);

alter table Users
    add LocationId int null;

alter table Locations
    modify Id int auto_increment;



