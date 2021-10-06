create table Users
(
    UserId                    int auto_increment,
    Username                  tinytext             not null,
    IconHash                  text                 null,
    Game                      int        default 0 not null,
    Lists                     int        default 0 not null,
    HeartCount                int        default 0 not null,
    YayHash                   text                 null,
    BooHash                   text                 null,
    Biography                 text                 null,
    ReviewCount               int        default 0 not null,
    CommentCount              int        default 0 not null,
    PhotosByMeCount           int        default 0 not null,
    PhotosWithMeCount         int        default 0 not null,
    CommentsEnabled           tinyint(1) default 1 not null,
    FavouriteSlotCount        int        default 0 not null,
    FavouriteUserCount        int        default 0 not null,
    lolcatftwCount            int        default 0 not null,
    Pins                      text                 not null,
    StaffChallengeGoldCount   int        default 0 not null,
    StaffChallengeSilverCount int        default 0 not null,
    StaffChallengeBronzeCount int        default 0 not null,
    UsedSlots                 int        default 0 not null,
    constraint users_user_id_uindex
        unique (UserId)
);

alter table Users
    add primary key (UserId);