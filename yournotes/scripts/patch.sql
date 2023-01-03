/* patch

drop table clients;
create table clients (id_client int not null identity(1,1) primary key, client_key varchar(20) not null, interval_ss int not null
	, machine_ip varchar(16), machine_name varchar(50), session_id varchar(100), dt_session_id datetime
	, dt_ins datetime not null default getdate(), dt_refresh datetime not null);
create unique index idx_clients on clients (client_key);
create index idx_clients_2 on clients (session_id);

drop table clients_cmds;
create table clients_cmds (id_client_cmd int not null identity(1,1) primary key, client_key varchar(20) not null
	, cmd varchar(max) not null, dt_ins datetime not null default getdate());

alter table dn_tasks add dt_lwt datetime
alter table dn_folders add dt_lwt datetime
alter table dn_files add dt_lwt datetime

*/

select * from dn_folders
select * from dn_tasks
select * from dn_files
dn_tasks_notes
dn_files_contents
users

select t.synch_folder_id, t.task_id, t.title, f.folder_id as parent_folder_id, t.folder_id
  , sf.local_path + isnull(replace(dbo.folder_path(t.folder_id), '/', '\'), '') as folder_path
  , t.file_id, ft.type_content, ft.open_comment, ft.close_comment
  , sf.local_path + replace(isnull(dbo.folder_path(f.folder_id), '\') + f.file_name, '/', '\') as file_path
  , tn.file_id as file_id_notes
  , sf.local_path + replace(isnull(dbo.folder_path(fn.folder_id), '\') + fn.file_name, '/', '\') as file_path_notes
 from dn_tasks t
 left join synch_folders sf on sf.synch_folder_id = t.synch_folder_id
 left join dn_files f on f.file_id = t.file_id
 left join dn_files_types ft on ft.extension = f.extension
 left join dn_tasks_notes tn on tn.task_id = t.task_id
 left join dn_files fn on fn.file_id = tn.file_id
 where t.task_id = 1480
  