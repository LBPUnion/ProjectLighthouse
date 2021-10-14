create table Tokens
(
    TokenId   int,
    UserId    int  not null,
    UserToken text not null
);

create unique index Tokens_TokenId_uindex
    on Tokens (TokenId);

alter table Tokens
    add constraint Tokens_pk
        primary key (TokenId);

alter table Tokens
    modify TokenId int auto_increment;
