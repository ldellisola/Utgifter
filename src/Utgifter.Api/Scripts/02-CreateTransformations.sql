create table Rules (
    Id uuid not null,
    ExpectedStore varchar(200) not null,
    NewStore varchar(200),
    NewCategory varchar(200),
    Trip bool,
    Shared bool,
    primary key (Id, ExpectedStore),
    unique (ExpectedStore)
    );


