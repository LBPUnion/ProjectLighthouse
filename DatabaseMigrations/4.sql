create table Tokens
(
    UserId int  not null,
    MMAuth text not null
);

create unique index Tokens_MMAuth_uindex
    on Tokens (MMAuth);
