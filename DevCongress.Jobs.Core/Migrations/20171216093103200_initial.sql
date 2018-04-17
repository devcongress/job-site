

-- user

CREATE TABLE public."user"
(
  id serial NOT NULL,
  username text NOT NULL, 
  password text NOT NULL, 
  last_login timestamp with time zone NULL, 
  lockout_end timestamp with time zone NULL, 
  ban_end timestamp with time zone NULL, 
  is_confirmed boolean NOT NULL, 
  is_active boolean NOT NULL DEFAULT true,
  tenant_id integer NOT NULL,
  created_by integer NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_by integer NOT NULL,
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  deleted_by integer,
  deleted_at timestamp with time zone,
  CONSTRAINT user_pkey PRIMARY KEY (id),
  CONSTRAINT user_username_check_len CHECK (char_length(username) >= 0 AND char_length(username) <= 50),
  CONSTRAINT user_password_check_len CHECK (char_length(password) >= 0 AND char_length(password) <= 255),
  CONSTRAINT user_username_key UNIQUE (username),
  CONSTRAINT user_tenant_id_fkey FOREIGN KEY (tenant_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_created_by_fkey FOREIGN KEY (created_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_updated_by_fkey FOREIGN KEY (updated_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_deleted_by_fkey FOREIGN KEY (deleted_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT

);


CREATE INDEX
  ON public."user" (tenant_id);

CREATE INDEX
  ON public."user" (created_by);
  
CREATE INDEX
  ON public."user" (updated_by);

CREATE INDEX
  ON public."user" (deleted_by);

-- audit log

CREATE TABLE public."audit_log"
(
  id bigserial NOT NULL,
  user_id integer NOT NULL,
  action_name text NOT NULL,
  description text NOT NULL,
  object_name character varying(254) NOT NULL,
  object_data jsonb NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT audit_log_pkey PRIMARY KEY (id),
  CONSTRAINT audit_log_action_name_check CHECK (char_length(action_name) <= 50),
  CONSTRAINT audit_log_description_check CHECK (char_length(description) <= 255),
  CONSTRAINT audit_log_object_name_check CHECK (char_length(object_name) <= 255),
  CONSTRAINT audit_log_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT
);

-- user audit log

CREATE TABLE public."user_audit_log"
(
  target_user_id integer NOT NULL,
  CONSTRAINT user_audit_log_pkey PRIMARY KEY (id),
  CONSTRAINT user_audit_log_target_user_id_fkey FOREIGN KEY (target_user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_audit_log_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT
)
INHERITS (public.audit_log);


CREATE INDEX
  ON public."user_audit_log" (user_id);
  
CREATE INDEX
  ON public."user_audit_log" (target_user_id);

-- user profile

CREATE TABLE public."user_profile"
(
  id serial NOT NULL,
  user_id integer NOT NULL,
  name text NOT NULL, 
  
  company_email text NULL, 
  company_website text NULL, 
  company_description text NULL, 

  created_by integer NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_by integer NOT NULL,
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT user_profile_pkey PRIMARY KEY (id),  
  CONSTRAINT item_company_email_check_len CHECK (char_length(company_email) >= 0 AND char_length(company_email) <= 60),
  CONSTRAINT item_company_website_check_len CHECK (char_length(company_website) >= 0 AND char_length(company_website) <= 60),
  CONSTRAINT item_company_description_check_len CHECK (char_length(company_description) >= 0 AND char_length(company_description) <= 500),
  CONSTRAINT user_profile_name_check_len CHECK (char_length(name) >= 1 AND char_length(name) <= 50),
  CONSTRAINT user_profile_user_id_key UNIQUE (user_id),
  CONSTRAINT user_profile_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_profile_created_by_fkey FOREIGN KEY (created_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT user_profile_updated_by_fkey FOREIGN KEY (updated_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT

);


CREATE INDEX
  ON public."user_profile" (created_by);
  
CREATE INDEX
  ON public."user_profile" (updated_by);

-- role

CREATE TABLE public."role"
(
  id serial NOT NULL,
  name text NOT NULL, 
  description text NOT NULL, 
  is_active boolean NOT NULL DEFAULT true, 
  created_by integer NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  updated_by integer NOT NULL,
  updated_at timestamp with time zone NOT NULL DEFAULT now(),
  deleted_by integer,
  deleted_at timestamp with time zone,
  CONSTRAINT role_pkey PRIMARY KEY (id), 
  CONSTRAINT role_name_key UNIQUE (name), 
  CONSTRAINT role_name_check_len CHECK (char_length(name) >= 3 AND char_length(name) <= 255),
  CONSTRAINT role_description_check_len CHECK (char_length(description) >= 0 AND char_length(description) <= 255),
  CONSTRAINT role_created_by_fkey FOREIGN KEY (created_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_updated_by_fkey FOREIGN KEY (updated_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_deleted_by_fkey FOREIGN KEY (deleted_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT

);


CREATE INDEX
  ON public."role" (created_by);
  
CREATE INDEX
  ON public."role" (updated_by);

CREATE INDEX
  ON public."role" (deleted_by);

-- role audit log

CREATE TABLE public."role_audit_log"
(
  target_role_id integer NOT NULL,
  CONSTRAINT role_audit_log_pkey PRIMARY KEY (id),
  CONSTRAINT role_audit_log_target_role_id_fkey FOREIGN KEY (target_role_id)
      REFERENCES public."role" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_audit_log_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT
)
INHERITS (public.audit_log);


CREATE INDEX
  ON public."role_audit_log" (user_id);
  
CREATE INDEX
  ON public."role_audit_log" (target_role_id);

-- permission

CREATE TABLE public."permission"
(
  id serial NOT NULL,
  name text NOT NULL, 
  description text NOT NULL, 
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT permission_pkey PRIMARY KEY (id),
  CONSTRAINT permission_name_key UNIQUE (name)
);

--- pivot tables


-- role_user

CREATE TABLE public."role_user"
(
  id serial,
  role_id integer NOT NULL,
  user_id integer NOT NULL,
  created_by integer NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT role_user_pkey PRIMARY KEY (id),
  CONSTRAINT role_user_created_by_fkey FOREIGN KEY (created_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_user_role_id_fkey FOREIGN KEY (role_id)
      REFERENCES public.role (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_user_user_id_fkey FOREIGN KEY (user_id)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT role_user_key UNIQUE (user_id, role_id)
);
  
CREATE INDEX
  ON public."role_user" (created_by);

-- permission_role

CREATE TABLE public."permission_role"
(
  id serial,
  role_id integer NOT NULL,
  permission_id integer NOT NULL,
  created_by integer NOT NULL,
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT permission_role_pkey PRIMARY KEY (id),
  CONSTRAINT permission_role_created_by_fkey FOREIGN KEY (created_by)
      REFERENCES public."user" (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT permission_role_permission_id_fkey FOREIGN KEY (permission_id)
      REFERENCES public.permission (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT permission_role_role_id_fkey FOREIGN KEY (role_id)
      REFERENCES public.role (id) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT permission_role_key UNIQUE (role_id, permission_id)
);

CREATE INDEX
  ON public."permission_role" (created_by);
  
-- magic_token

CREATE TABLE public."magic_token"
(
  token uuid NOT NULL, 
  email text NOT NULL, 
  metadata jsonb NULL, 
  purpose text NOT NULL, 
  expires_at timestamp with time zone NOT NULL, 
  used_at timestamp with time zone NULL, 
  created_at timestamp with time zone NOT NULL DEFAULT now(),
  CONSTRAINT magic_token_pkey PRIMARY KEY (token),  
  CONSTRAINT magic_token_email_check_len CHECK (char_length(email) >= 0 AND char_length(email) <= 255)
);

--- seed data

INSERT INTO
  public."user"
    (id, username, password, is_confirmed, is_active, tenant_id, created_by, updated_by)
  VALUES
    (1, 'system', '', true, true, 1, 1, 1);
    
INSERT INTO
  public."user"
    (id, username, password, is_confirmed, is_active, tenant_id, created_by, updated_by)
  VALUES
    (2, 'admin', '$2y$10$OAZWBCcNG5jSZZ8lA4vxjuVXwvaHqhDUX5dClEmbbmAwAB6U2kLWO', true, true, 1, 1, 1);

INSERT INTO
  public.user_profile
    (id, user_id, name, created_by, updated_by)
  VALUES
    (1, 1, 'System', 1, 1);
    
INSERT INTO
  public.user_profile
    (id, user_id, name, created_by, updated_by)
  VALUES
    (2, 2, 'Admin', 1, 1);
    
INSERT INTO
  public."role"
    (id, name, description, is_active, created_by, updated_by)
  VALUES
    (1, 'superuser', 'Full and complete access', true, 1, 1);
    
INSERT INTO
  public."role"
    (id, name, description, is_active, created_by, updated_by)
  VALUES
    (2, 'basic_user', 'Basic user', true, 1, 1);

INSERT INTO
  public."permission"
    (id, name, description)
  VALUES
    (1, '*', 'Superuser');
    
INSERT INTO
  public."role_user"
    (id, role_id, user_id, created_by)
  VALUES
    (1, 1, 1, 1);
    
INSERT INTO
  public."role_user"
    (id, role_id, user_id, created_by)
  VALUES
    (2, 1, 2, 1);

INSERT INTO
  public."permission_role"
    (id, role_id, permission_id, created_by)
  VALUES
    (1, 1, 1, 1);

--- fix sequences

SELECT SETVAL('public.user_id_seq', COALESCE(MAX(id), 1) ) FROM public."user";
SELECT SETVAL('public.user_profile_id_seq', COALESCE(MAX(id), 1) ) FROM public.user_profile;
SELECT SETVAL('public.role_id_seq', COALESCE(MAX(id), 1) ) FROM public.role;
SELECT SETVAL('public.permission_id_seq', COALESCE(MAX(id), 1) ) FROM public.permission;
SELECT SETVAL('public.role_user_id_seq', COALESCE(MAX(id), 1) ) FROM public.role_user;
SELECT SETVAL('public.permission_role_id_seq', COALESCE(MAX(id), 1) ) FROM public.permission_role;

--- user permissions


INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.add', 'Add user for current user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.add.su', 'Add user for any user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.fetch', 'Retrieve account for user created by the current user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.fetch.su', 'Retrieve account created by any user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.getLogs', 'Retrieve logs for user created by the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.getLogs.su', 'Retrieve logs for account created by any user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.list', 'List acccounts created by the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.list.su', 'List accounts created by any user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.restore', 'Restore a trashed account created by the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.restore.su', 'Restore a trashed account created by any user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.password.set', 'Set password for account created by the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.password.set.su', 'Set password for account created by any user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.trash', 'Trash account belonging to the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.trash.su', 'Trash account belonging any user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.update', 'Update account created by the current user/tenant');

    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.update.su', 'Update account created by any user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.profile.update', 'Update account profile created by the current user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.profile.update.su', 'Update account profile created by any user/tenant');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.roles.update', 'Update roles for account created by the current user/tenant');
    
INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('user.roles.update.su', 'Update roles for account created by any user/tenant');

-- role permissions


INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.add', 'Add role');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.fetch', 'Retrieve role');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.getLogs', 'Retrieve logs for roles');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.list', 'List roles');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.restore', 'Restore a trashed role');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.trash', 'Trash role');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.update', 'Update role');

INSERT INTO
  public.permission
    (name, description)
  VALUES
    ('role.permissions.update', 'Update role permissions');



