create table categories_products
(
	id serial,
	name varchar(50) not null,
	primary key(id)
);

select * from categories_products;

create table suplitiers
(
	id serial,
	name varchar(50) not null,
	Primary key(id)
);

select * from suplitiers;

create table manufactures
(
	id serial,
	name varchar(50) not null,
	primary key(id)
);

select * from manufactures;

create table title
(
	id serial,
	name varchar(50) not null,
	primary key(id)
);

select * from title;

create table products
(
	id serial,
	articl varchar(50) not null,
	title_id integer not null,
	unit_of_measurement varchar(50) not null,
	price numeric(15,2) not null,
	suplitiers_id integer not null,
	manufactures_id integer not null,
	categories_products integer not null,
	discount integer not null,
	qty integer not null,
	description text not null,
	image varchar(100),
	primary key(id),
	foreign key(suplitiers_id) references suplitiers(id),
	foreign key(manufactures_id) references manufactures(id),
	foreign key(categories_products) references categories_products(id),
	foreign key(title_id) references title(id)	
);

select * from products;

create table individual
(
	id serial,
	last_name varchar(50) not null,
	ferst_name varchar(50) not null,
	midel_name varchar(50),
	primary key(id)
);

select * from individual;

create table role_users
(
	id serial,
	name varchar(50) not null,
	primary key(id)
);

select * from role_users;

create table users
(
	id serial,
	role_users_id integer not null,
	individual_id integer not null,
	login varchar(50) not null,
	password varchar(50) not null,
	primary key(id),
	foreign key(role_users_id) references role_users(id),
	foreign key(individual_id) references individual(id)
);

select * from users;

create table pic_up_poins
(
	id serial,
	Mailing_address varchar(50) not null,
	city varchar(50) not null,
	street varchar(50) not null,
	street_number varchar(50),
	primary key(id)
);

select * from pic_up_poins;

create table status
(
	id serial,
	name varchar(50) not null,
	primary key(id)
);

select * from status;

create table orders
(
	id serial,
	orders_date date not null,
	delivery_date date not null,
	pic_up_poins_id integer not null,
	users_id integer not null,
	code varchar(50) not null,
	status_id integer not null,
	primary key(id),
	foreign key(pic_up_poins_id) references pic_up_poins(id),
	foreign key(users_id) references users(id),
	foreign key(status_id) references status(id)
)

select * from orders;

create table orders_datalse
(
	id serial,
	orders_id integer not null,
	products_id integer not null,
	qty integer not null,
	primary key(id),
	foreign key(products_id) references products(id),
	foreign key(orders_id) references orders(id)
);

select * from orders_datalse;








