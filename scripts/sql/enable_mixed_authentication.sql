-- Run as a SQL Server administrator on the selected instance.
-- This change takes effect after restarting the instance service.
-- Restarting the service interrupts active connections to that instance.
USE master;
GO

EXEC master.dbo.xp_instance_regwrite
    N'HKEY_LOCAL_MACHINE',
    N'Software\Microsoft\MSSQLServer\MSSQLServer',
    N'LoginMode',
    REG_DWORD,
    2;
GO
