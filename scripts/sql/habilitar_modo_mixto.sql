-- Ejecutar como administrador de SQL Server en la instancia elegida.
-- Este cambio solo toma efecto despues de reiniciar el servicio de la instancia.
-- El reinicio interrumpe las conexiones activas a esa instancia.
USE master;
GO

EXEC master.dbo.xp_instance_regwrite
    N'HKEY_LOCAL_MACHINE',
    N'Software\Microsoft\MSSQLServer\MSSQLServer',
    N'LoginMode',
    REG_DWORD,
    2;
GO
