create table ProfileCardLocations
(
    UserId    int not null,
    LocationX int not null,
    LocationY int not null
);

create unique index ProfileCardLocations_UserId_uindex
    on Locations (Id);

alter table Locations
    add constraint ProfileCardLocations_pk
        primary key (Id);

alter table Users
    add LocationId int null;

