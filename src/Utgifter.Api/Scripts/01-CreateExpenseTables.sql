
create table Expenses
(
    Id uuid primary key,
    Person varchar(200) not null,
    Date date not null,
    Amount decimal(18, 2) not null,
    OriginalCurrency varchar(50) not null,
    City varchar(200) not null,
    Store varchar(200) not null,
    
    Trip bool not null,
    Shared bool not null,
    Category varchar(200)
    );
