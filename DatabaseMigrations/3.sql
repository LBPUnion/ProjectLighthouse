create table Comments
(
    CommentId    int,
    PosterUserId int           not null,
    TargetUserId int           not null,
    Timestamp    bigint        not null,
    ThumbsUp     int default 0 not null,
    ThumbsDown   int default 0 not null,
    Message      longtext      not null
);

create unique index Comments_CommentId_uindex
    on Comments (CommentId);

alter table Comments
    add constraint Comments_pk
        primary key (CommentId);

alter table Comments
    modify CommentId int auto_increment;

