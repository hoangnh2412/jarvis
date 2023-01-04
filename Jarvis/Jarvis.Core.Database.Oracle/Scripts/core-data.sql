INSERT INTO DBDEV.CORE_TENANT ("Id","Code","Name","IdParent","Path","Server","Database","DbConnectionString","Theme","IsEnable","ExpireDate","CreatedAt","CreatedAtUtc","CreatedBy","UpdatedAt","UpdatedAtUtc","UpdatedBy","DeletedAt","DeletedAtUtc","DeletedBy","DeletedVersion") VALUES
	 (1,NULL,'00000000000',NULL,'5bfa6069-2968-4ce3-b811-3daccf7711b2',NULL,NULL,NULL,NULL,1,NULL,TIMESTAMP'2022-02-24 22:53:04.030752',TIMESTAMP'2022-02-24 15:53:04.030826',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);

INSERT INTO DBDEV.CORE_TENANT_HOST ("Id","Code","HostName","DeletedVersion") VALUES
	 (1,NULL,'localhost:8080',NULL);

INSERT INTO DBDEV.CORE_TENANT_INFO ("Id","Code","IsCurrent","TaxCode","Address","City","Country","District","FullNameVi","FullNameEn","LegalName","Fax","BusinessType","Emails","Phones","Metadata") VALUES
	 (1,NULL,1,'00000000000','Ha Noi','#','#','#','EVN','EVN',NULL,NULL,NULL,NULL,NULL,NULL);

INSERT INTO DBDEV.CORE_USER ("Id","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp","PhoneNumber","PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnd","LockoutEnabled","AccessFailedCount","TenantCode","CreatedAt","CreatedAtUtc","CreatedBy","UpdatedAt","UpdatedAtUtc","UpdatedBy","DeletedAt","DeletedAtUtc","DeletedBy","DeletedVersion") VALUES
	 (NULL,'root','ROOT',NULL,NULL,0,'AQAAAAEAACcQAAAAEF4msskKRMeCxIiBUkROlIWMxbbbkxboVyY31iMDJiTZzLs9bmn75yALulIXkN7YMA==','V3RWSYUI4AQDFIA3ELVGOFUHY5NFRJQJ','4cfaa9e4-bb6d-44b4-9135-275f318552e1',NULL,0,0,NULL,1,0,NULL,TIMESTAMP'2022-02-24 22:53:33.729661',TIMESTAMP'2022-02-24 15:53:33.729754',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL);

INSERT INTO DBDEV.CORE_USER_CLAIM ("Id","UserId","ClaimType","ClaimValue") VALUES
	 (1,NULL,'Special_DoEnything','Owner|None');

INSERT INTO DBDEV.CORE_USER_INFO ("Id","FullName","AvatarPath") VALUES
	 (NULL,'Root',NULL);