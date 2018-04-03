

DELETE FROM
  public.permission
    WHERE name
    IN (
      'role.add',
      'role.fetch',
      'role.getLogs',
      'role.list',
      'role.restore',
      'role.trash',
      'role.update',
      'role.permissions.update',

      'user.add',
      'user.fetch.su',
      'user.getLogs.su',
      'user.list',
      'user.list.su',
      'user.restore',
      'user.restore.su',
      'user.password.set.su',
      'user.trash',
      'user.trash.su',
      'user.update',
      'user.profile.update.su',
      'user.roles.update'
    );


DROP TABLE public."magic_token";

DROP TABLE public."permission_role";
DROP TABLE public."role_user";

DROP TABLE public."permission";

DROP TABLE public."role_audit_log";
DROP TABLE public."role";

DROP TABLE public."user_profile";
DROP TABLE public."user_audit_log";
DROP TABLE public."audit_log";
DROP TABLE public."user";
