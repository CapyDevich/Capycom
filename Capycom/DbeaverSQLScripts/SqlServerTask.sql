USE msdb;
GO

-- Создание задания SQL Server Agent
EXEC dbo.sp_add_job
    @job_name = N'Удаление старых логов Serilog',
    @enabled = 1,
    @notify_level_eventlog = 0,
    @notify_level_email = 2,
    @notify_level_netsend = 2,
    @notify_level_page = 2,
    @delete_level= 0,
    @description = N'Удаляет записи из таблицы логов Serilog, которые старше 30 дней.',
    @category_name = N'[Uncategorized (Local)]',
    @owner_login_name = N'sa';

-- Создание шага задания
EXEC sp_add_jobstep
    @job_name = N'Удаление старых логов Serilog',
    @step_name = N'Удаление старых записей',
    @subsystem = N'TSQL',
    @command = N'DELETE FROM CPCM_LogEvents WHERE TimeStamp < DATEADD(day, -30, GETDATE());',
    @retry_attempts = 5,
    @retry_interval = 5;

-- Назначение задания на планировщик
EXEC dbo.sp_add_jobschedule
    @job_name = N'Удаление старых логов Serilog',
    @name = N'Ежедневное удаление',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_date = 20240404; 
    -- махнуть на текущую дату