create table outbox_messages (
  id bigint constraint pk_outbox_messages primary key,
  occured_on timestamp not null,
  type varchar(255) not null,
  payload json not null
);

create table outbox_messages_processed (
  id bigint constraint pk_outbox_messages_processed primary key,
  occured_on timestamp not null,
  type varchar(255) not null,
  payload json not null,
  processed_on timestamp not null
);

